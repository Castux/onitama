using System;
using System.Collections.Generic;

namespace Onitama
{
	public class Solver
	{
		public const int MaxScore = 100;

		public int LeavesVisited { private set; get; }
		public int NodesVisited { private set; get; }
		public int MemHits { private set; get; }
		public int Value { private set; get; }

		private GameState root;
		private int maxDepth;
		private List<List<Move>> moveLists;

		public TranspositionTable table;

		private List<Move> tmpMoves;

		public Solver(GameState game, int depth)
		{
			LeavesVisited = 0;
			NodesVisited = 0;
			MemHits = 0;
			root = game;
			maxDepth = depth;

			table = new TranspositionTable(26);

			// Preallocate all the move lists

			moveLists = new List<List<Move>>();
			for (int i = 0; i <= depth; i++)
			{
				moveLists.Add(new List<Move>());
			}

			tmpMoves = new List<Move>();
		}

		public void ComputeValue()
		{
			Value = ComputeValue(root, maxDepth, -MaxScore, MaxScore);
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

		private int ComputeValue(GameState state, int depth, int alpha, int beta)
		{
			var startAlpha = alpha;


			NodesVisited++;
			if (NodesVisited % 100000 == 0)
				Console.WriteLine("Nodes visited: " + NodesVisited);

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
			state.ComputeValidMoves(moves);

			if (depth == 0 || moves.Count == 0)
				return ComputeLeafValue(state);

			// Check most promising moves first: previously computed best move, win, capture, normal

			tmpMoves.Clear();

			foreach (var m in moves)
				if (m.Equals(ttBestMove))
					tmpMoves.Add(m);

			foreach (var m in moves)
			{
				if (m.quality == (byte)MoveQuality.Win && !m.Equals(ttBestMove))
					tmpMoves.Add(m);
			}

			foreach (var m in moves)
			{
				if (m.quality == (byte)MoveQuality.Capture && !m.Equals(ttBestMove))
					tmpMoves.Add(m);
			}

			foreach (var m in moves)
			{
				if (m.quality == (byte)MoveQuality.Normal && !m.Equals(ttBestMove))
					tmpMoves.Add(m);
			}

			moves.Clear();
			moves.AddRange(tmpMoves);

			int bestMoveIndex = -1;

			// Do the thing!

			for (int i = 0; i < moves.Count; i++)
			{
				var move = moves[i];

				var childState = state.ApplyMove(move);
				var childValue = -ComputeValue(childState, depth - 1, -beta, -alpha);

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
			LeavesVisited++;
			if (LeavesVisited % 1000000 == 0)
				Console.WriteLine("Leaves visited: " + LeavesVisited);

			if (state.player == Player.Top)
			{
				if (state.board.BottomWon())
					return -MaxScore;

				return state.board.TopStudentCount() - state.board.BottomStudentCount();
			}

			else
			{
				if (state.board.TopWon())
					return -MaxScore;

				return state.board.BottomStudentCount() - state.board.TopStudentCount();
			}
		}
	}
}
