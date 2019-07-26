using System;
using System.Threading;
using System.Collections.Generic;

namespace Onitama
{
	public class Solver
	{
		public const int WinScore = 125;
		public const int PawnScore = 25;

		public int Value { private set; get; }
		public DateTime StartTime { private set; get; }
		public Stats Stats { private set; get; }

		public bool interrupt = false;

		private GameState root;
		private int maxDepth;
		
		private TranspositionTable table1;
		private TranspositionTable table2;
		
		public Solver(int maxDepth, double ttSize = 2)
		{
			// Parameters
			
			this.maxDepth = maxDepth;

			// Allocs

			Stats = new Stats();
			table1 = new TranspositionTable(gbytes: ttSize / 2);
			table2 = new TranspositionTable(gbytes: ttSize / 2);
		}

		public void Start(GameState state, TimeSpan timeout)
		{
			var thread = new Thread(() => Start(state));
			thread.Start();

			Thread.Sleep(timeout);
			interrupt = true;

			thread.Join();
		}

		public void Start(GameState state)
		{
			Stats.StartTimer();

			root = state;
			IterativeSearch();

			Stats.StopTimer();
		}

		public List<Move> PrincipalVariation()
		{
			var res = new List<Move>();
			var g = root;
			while(true)
			{
				var entry = table1.Get(g);
				if (!entry.HasValue)
					entry = table2.Get(g);

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
				Value = ComputeValue(root, depth, 0, -int.MaxValue, int.MaxValue);
				Console.Write("Depth {0}: {1} {2:0.00}\t", depth, Value, (DateTime.Now - StartTime).TotalSeconds);
				foreach (var m in PrincipalVariation())
					Console.Write(" | " + m);

				Console.WriteLine();

				if (interrupt)
					break;

				if (Math.Abs(Value) == WinScore)
					break;
			}
		}

		private int ComputeValue(GameState state, int depth, int ply, int alpha, int beta)
		{
			Stats.NodeVisited(ply);

			if(state.board.BottomWon() || state.board.TopWon())
			{
				Stats.LeafVisited(ply);
				return ComputeLeafValue(state);
			}

			if (depth == 0)
			{
				Stats.LeafVisited(ply);
				return QuiescenceSearch(state, alpha, beta, ply);
			}

			var startAlpha = alpha;

			// Check transposition table

			Stats.TTLookup(ply);

			var ttEntry = table1.Get(state);
			if (!ttEntry.HasValue)
				ttEntry = table2.Get(state);

			Move? ttBestMove = null;

			if (ttEntry.HasValue)
			{
				Stats.TTHit(ply);

				if (ttEntry.Value.depth >= depth)
				{
					Stats.TTGotValue(ply);

					switch (ttEntry.Value.flag)
					{
						case TranspositionTable.Flag.Exact:
							Stats.TTCutoff(ply);
							return ttEntry.Value.value;
						case TranspositionTable.Flag.Lower:
							alpha = Math.Max(alpha, ttEntry.Value.value);
							break;
						case TranspositionTable.Flag.Upper:
							beta = Math.Min(beta, ttEntry.Value.value);
							break;
					}

					if (alpha >= beta)
					{
						Stats.TTCutoff(ply);

						return ttEntry.Value.value;
					}
				}

				ttBestMove = ttEntry.Value.move;
			}

			// TODO: see if we should pass state as reference? Is that even a thing?
			// Negamax!

			var value = int.MinValue;

			var moveSorter = new MoveSorter(ply, state, ttBestMove);
			var bestMove = new Move();

			// Do the thing!

			Stats.Recursed(ply);

			int i = 0;
			while(true)
			{
				var moveIndex = moveSorter.GetNextIndex();
				if (moveIndex < 0)
					break;

				var move = moveSorter.GetMove(moveIndex);

				Stats.MoveExplored(ply);

				var childState = state.ApplyMove(move);
				int childValue;

				if (i == 0)								// Principal Variation Search: try to prove that the best move is indeed the best
				{
					childValue = -ComputeValue(childState, depth - 1, ply + 1, -beta, -alpha);
					i++;
				}
				else
				{
					Stats.PVSAttempt(ply);

					childValue = -ComputeValue(childState, depth - 1, ply + 1, -alpha-1, -alpha);

					if (childValue > alpha && childValue < beta)
					{
						Stats.PVSRecompute(ply);

						childValue = -ComputeValue(childState, depth - 1, ply + 1, -beta, -alpha);
					}
				}

				if (childValue > value)
				{
					value = childValue;
					bestMove = move;
				}

				if (value > alpha)
				{
					alpha = value;
				}

				if (alpha >= beta)
					break;

				if (interrupt)
					break;
			}

			// Did we cutoff using only the best move?

			if (!moveSorter.GeneratedAllMoves)
			{
				Stats.BestMoveCutoff(ply);
			}

			// Save in transposition table

			TranspositionTable.Flag flag;

			if (value <= startAlpha)
				flag = TranspositionTable.Flag.Upper;
			else if (value >= beta)
				flag = TranspositionTable.Flag.Lower;
			else
				flag = TranspositionTable.Flag.Exact;

			if(!table1.AddIfHigherDepth(state, bestMove, value, depth, flag))
			{
				table2.Add(state, bestMove, value, depth, flag);
			}

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
				score += Positioning.Center(state.board);
			}
			
			// Negate if it was Bottom's turn

			if (state.player == Player.Bottom)
				score = -score;

			return score;
		}

		private int QuiescenceSearch(GameState state, int alpha, int beta, int ply)
		{
			Stats.QuiescenceNodeVisited();

			var value = ComputeLeafValue(state);

			if (value > alpha)
				alpha = value;

			if (alpha >= beta)
				return value;

			var moveSorter = new MoveSorter(ply, state, null, winAndCaptureOnly: true);

			while(true)
			{
				var moveIndex = moveSorter.GetNextIndex();
				if (moveIndex < 0)
					break;

				var m = moveSorter.GetMove(moveIndex);

				if(m.quality == (byte)MoveQuality.Win || m.quality == (byte)MoveQuality.Capture)
				{
					var childState = state.ApplyMove(m);
					var childValue = -QuiescenceSearch(childState, -beta, -alpha, ply + 1);

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
	}
}
