using System;
using System.Collections.Generic;

namespace Onitama
{
	public class Solver
	{
		public const int WinScore = 125;
		public const int PawnScore = 25;

		public Stats Stats { private set; get; }

		private bool interrupt = false;
		
		private List<List<Move>> moveLists;

		private TwoTieredTable table;
		private List<List<Move>> quiescenceMoves;

		private int workerIndex;

		public Solver(double ttSize, int workerIndex = 0) :
			this(new TwoTieredTable(gbytes: ttSize), workerIndex)
		{
		}

		public Solver(TwoTieredTable table, int workerIndex = 0)
		{
			// Parameters
			
			this.table = table;
			this.workerIndex = workerIndex;

			// Allocs. We'll never need more that 50 depths, right?

			Stats = new Stats();

			moveLists = new List<List<Move>>();
			while (moveLists.Count < 50)
				moveLists.Add(new List<Move>());

			quiescenceMoves = new List<List<Move>>();
			while (quiescenceMoves.Count < 50)
				quiescenceMoves.Add(new List<Move>());
		}

		public int ComputeValue(GameState state, int depth, out Move bestMove)
		{
			interrupt = false;

			var value = ComputeValue(state, depth, 0, int.MinValue, int.MaxValue);
			bestMove = table.Get(state).Value.move;

			return value;
		}

		public void Interrupt()
		{
			interrupt = true;
		}

		public bool Interrupted
		{
			get { return interrupt; }
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

			// Although rare, it can happen that no moves exist for a position
			// In this case, we lose.

			if(moves.Count == 0)
			{
				return -WinScore;
			}

			// Do the thing!

			int bestMoveIndex = -1;
			
			Stats.Recursed(ply);

			for(int i = 0; i < moves.Count; i++)
			{
				var moveIndex = (ply == 0) ? (i + workerIndex) % moves.Count : i;
				var move = moves[moveIndex];
				
				var childState = state.ApplyMove(move);
				int childValue;

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
