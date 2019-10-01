﻿using System;
using System.Threading;
using System.Collections.Generic;

namespace Onitama
{
	public class Solver
	{
		public const int WinScore = 125;
		public const int PawnScore = 25;

		public Stats Stats { private set; get; }

		private bool interrupt = false;
		private Thread thread;

		private GameState root;
		private int maxDepth;
		
		private List<List<Move>> moveLists;

		private TwoTieredTable table;
		private StateLocker locker;
		private List<List<Move>> quiescenceMoves;


		public Solver(int maxDepth, double ttSize) :
			this(maxDepth, new TwoTieredTable(gbytes: ttSize))
		{
		}

		public Solver(int maxDepth, TwoTieredTable table, StateLocker locker = null)
		{
			// Parameters
			
			this.maxDepth = maxDepth;
			this.table = table;
			this.locker = locker;

			// Allocs

			Stats = new Stats();

			moveLists = new List<List<Move>>();
			for (int i = 0; i <= maxDepth; i++)
				moveLists.Add(new List<Move>());

			quiescenceMoves = new List<List<Move>>();
			for (int i = 0; i < 20; i++)
				quiescenceMoves.Add(new List<Move>());
		}

		public void Run(GameState state, TimeSpan timeout)
		{
			RunInBackground(state);

			thread.Join(timeout);

			InterruptBackground();
		}

		public void RunInBackground(GameState state)
		{
			interrupt = false;

			thread = new Thread(() => Run(state));
			thread.Start();
		}

		public void InterruptBackground()
		{
			interrupt = true;
			thread.Join();
		}

		public void Run(GameState state)
		{
			Stats.StartTimer();

			root = state;
			IterativeSearch();

			Stats.StopTimer();
		}

		public Move BestMove()
		{
			return table.Get(root).Value.move;
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

					if(res.Contains(move))
					{
						break;
					}

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
			var startTime = DateTime.Now;

			for(var depth = 1; depth <= maxDepth; depth++)
			{
				var value = ComputeValue(root, depth, 0, -int.MaxValue, int.MaxValue);
				Console.Write("Depth {0,2:##}: {1} {2:0.00}\t", depth, value, (DateTime.Now - startTime).TotalSeconds);

				Console.Write(BestMove());

				Console.WriteLine();

				if (interrupt)
					break;

				if (Math.Abs(value) == WinScore)
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
				return QuiescenceSearch(state, alpha, beta, 0);
			}

			var startAlpha = alpha;

			// Check transposition table

			Stats.TTLookup(ply);

			var ttEntry = table.Get(state);
			var ttBestMove = new Move();

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

			if (locker != null)
				locker.Lock(state);

			var value = int.MinValue;

			var moves = moveLists[depth];
			moves.Clear();

			// Try only the best move first

			bool generatedAllMoves = false;

			if (ttEntry.HasValue)
			{
				moves.Add(ttBestMove);
			}
			else
			{
				state.AddValidMoves(moves);
				generatedAllMoves = true;
			}

			// Do the thing!

			int bestMoveIndex = -1;
			int delayedMoves = 0;
			bool firstPass = true;

			Stats.Recursed(ply);

			int i = -1;
			while(true)
			{
				i++;
				if(i >= moves.Count)
				{
					if (firstPass)
					{
						firstPass = false;
						i = 0;
					}
					else
						break;
				}

				var move = moves[i];

				if (i > 0 && move.Equals(ttBestMove))	// Best move is always tested first, but generated again. Skip it!
					continue;

				var childState = state.ApplyMove(move);
				int childValue;

				if (locker != null)
				{
					if (firstPass)
					{
						if (locker.IsLocked(childState))
						{
							delayedMoves |= (1 << i);
							continue;
						}
					}
					else
					{
						if ((delayedMoves & (1 << i)) == 0)
							continue;
					}
				}

				Stats.MoveExplored(ply);

				if (i == 0)								// Principal Variation Search: try to prove that the best move is indeed the best
				{
					childValue = -ComputeValue(childState, depth - 1, ply + 1, -beta, -alpha);
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
					bestMoveIndex = i;
				}

				if (value > alpha)
				{
					alpha = value;
				}

				if (alpha >= beta)
					break;

				if (interrupt)
					break;

				// If the best move hasn't caused a cutoff, generate the rest of the moves here

				if(!generatedAllMoves)
				{
					state.AddValidMoves(moves);
					generatedAllMoves = true;
				}
			}

			// Did we cutoff using only the best move?

			if (!generatedAllMoves)
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


			table.Add(state, moves[bestMoveIndex], value, depth, flag);

			if (locker != null)
				locker.Unlock(state);

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

		private int QuiescenceSearch(GameState state, int alpha, int beta, int depth)
		{
			Stats.QuiescenceNodeVisited();

			var value = ComputeLeafValue(state);

			if (value > alpha)
				alpha = value;

			if (alpha >= beta)
				return value;

			var moves = quiescenceMoves[depth];
			moves.Clear();
			state.AddValidMoves(moves, winAndCaptureOnly: true);

			for (int i = 0; i < moves.Count; i++)
			{
				var m = moves[i];

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
	}
}
