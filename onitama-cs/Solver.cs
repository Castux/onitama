using System;
using System.Collections.Generic;

namespace Onitama
{
	public class Solver
	{
		public const int WinScore = 125;
		public const int PawnScore = 25;

		public int LeavesVisited { private set; get; }
		public int NodesVisited { private set; get; }
		public int QuiescenceNodesVisited { get; private set; }
		public int MemHits { private set; get; }
		public int Value { private set; get; }
		public DateTime StartTime { private set; get; }

		private GameState root;
		private int maxDepth;
		private TimeSpan timeout;
		private int lastTimeoutCheck;

		private List<List<Move>> moveLists;

		private TranspositionTable table;
		private List<List<Move>> quiescenceMoves;
		
		public Solver(int maxDepth, TimeSpan timeout)
		{
			// Parameters
			
			this.maxDepth = maxDepth;
			this.timeout = timeout;

			// Allocs

			table = new TranspositionTable(26);

			moveLists = new List<List<Move>>();
			for (int i = 0; i <= maxDepth; i++)
				moveLists.Add(new List<Move>());

			quiescenceMoves = new List<List<Move>>();
			for (int i = 0; i < 20; i++)
				quiescenceMoves.Add(new List<Move>());
		}

		public void Start(GameState state)
		{
			root = state;

			// Stats

			LeavesVisited = 0;
			NodesVisited = 0;
			QuiescenceNodesVisited = 0;
			MemHits = 0;

			lastTimeoutCheck = 0;

			IterativeSearch();
		}

		public List<Move> PrincipalVariation()
		{
			var res = new List<Move>();
			var g = root;
			while(true)
			{
				var entry = table.Get(g);
				if(entry.HasValue)
				{
					var move = entry.Value.move;
					res.Add(move);

					g = g.ApplyMove(move);
				}
				else
				{
					break;
				}
			}

			return res;
		}

		private void IterativeSearch()
		{
			StartTime = DateTime.Now;

			Value = -int.MaxValue;

			for(var depth = 1; depth <= maxDepth; depth++)
			{
				Value = ComputeValue(root, depth, -int.MaxValue, int.MaxValue);
				Console.WriteLine("Depth " + depth + ": " + Value + " " + (DateTime.Now - StartTime).TotalSeconds);

				if (Timeouted())
					break;

				if (Math.Abs(Value) == WinScore)
					break;
			}
		}

		private int ComputeValue(GameState state, int depth, int alpha, int beta)
		{
			if (depth == 0 || state.board.BottomWon() || state.board.TopWon())
			{
				return QuiescenceSearch(state, alpha, beta, 0);
			}

			var startAlpha = alpha;

			NodesVisited++;

			// Check transposition table

			var ttEntry = table.Get(state);
			if (ttEntry.HasValue && ttEntry.Value.depth >= depth)
			{
				MemHits++;
                switch (ttEntry.Value.flag)
                {
                    case TranspositionTable.Flag.Exact:
                        return ttEntry.Value.value;
                    case TranspositionTable.Flag.Lower:
                        alpha = Math.Max(alpha, ttEntry.Value.value);
                        break;
                    case TranspositionTable.Flag.Upper:
                        beta = Math.Min(beta, ttEntry.Value.value);
                        break;
                }

                if (alpha >= beta)
					return ttEntry.Value.value;
			}

			var ttBestMove = new Move();
			if (ttEntry.HasValue)
				ttBestMove = ttEntry.Value.move;

			// TODO: see if we should pass state as reference? Is that even a thing?
			// Negamax!

			var value = int.MinValue;

			var moves = moveLists[depth];
			moves.Clear();

			if (ttEntry.HasValue)
			{
				moves.Add(ttBestMove);
			}
			state.AddValidMoves(moves);

			int bestMoveIndex = -1;

			// Do the thing!

			for (int i = 0; i < moves.Count; i++)
			{
				var move = moves[i];

				var childState = state.ApplyMove(move);
				int childValue;

				if (i == 0)
				{
					childValue = -ComputeValue(childState, depth - 1, -beta, -alpha);
				}
				else
				{
					childValue = -ComputeValue(childState, depth - 1, -alpha-1, -alpha);

					if(childValue > alpha && childValue < beta)
						childValue = -ComputeValue(childState, depth - 1, -beta, -alpha);
				}

				if (childValue > value)
				{
					value = childValue;
					bestMoveIndex = i;
				}

				if (value > alpha)
				{
					alpha = value;
				}

				if (alpha >= beta)
					break;

				if (Timeouted())
					break;
			}

			// Save in transposition table

			TranspositionTable.Flag flag;

			if (value <= startAlpha)
				flag = TranspositionTable.Flag.Upper;
			else if (value >= beta)
				flag = TranspositionTable.Flag.Lower;
			else
				flag = TranspositionTable.Flag.Exact;

			table.Add(state, moves[bestMoveIndex], value, depth, flag);

			return value;
		}

		private int ComputeLeafValue(GameState state)
		{
			int score;
			
			// Count in Top's perspective

			if (state.board.BottomWon())
			{
				score = -WinScore;
			}
			else if(state.board.TopWon())
			{
				score = WinScore;
			}
			else
			{
				score = (state.board.TopStudentCount() - state.board.BottomStudentCount()) * PawnScore;
				score += Positioning.TotalAdvance(state.board);
			}
			
			// Negate if it was Bottom's turn

			if (state.player == Player.Bottom)
				score = -score;

			return score;
		}

		private int QuiescenceSearch(GameState state, int alpha, int beta, int depth)
		{
			if (depth == 0)
				LeavesVisited++;
			else
				QuiescenceNodesVisited++;

			var value = ComputeLeafValue(state);

			if (value > alpha)
				alpha = value;

			if (alpha >= beta)
				return value;

			var moves = quiescenceMoves[depth];
			moves.Clear();
			state.AddValidMoves(moves);

			foreach(var m in moves)
			{
				if(m.quality == (byte)MoveQuality.Win || m.quality == (byte)MoveQuality.Capture)
				{
					var childState = state.ApplyMove(m);
					var childValue = -QuiescenceSearch(childState, -beta, -alpha, depth + 1);

					if (childValue > value)
						value = childValue;

					if (value > alpha)
						alpha = value;

					if (alpha >= beta)
						break;
				}
			}

			return value;
		}

		private bool Timeouted()
		{
			if(NodesVisited - lastTimeoutCheck > 100000)
			{
				lastTimeoutCheck = NodesVisited;
				return DateTime.Now - StartTime > timeout;
			}

			return false;
		}
	}
}
