using System;
using System.Collections.Generic;

namespace Onitama
{
	public class Solver
	{
		// Scores are saved a signed bytes in the transposition table,
		// so they should be in [-128,127].

		public const int WinScore = 125;
		public const int PawnScore = 25;
		public const int Infinity = 1000;

		private bool interrupt = false;
		
		private List<List<Move>> moveLists;

		private TranspositionTable table;
		private List<List<Move>> quiescenceMoves;

		private MoveLocker locker;

		public Solver(double ttSize) :
			this(new TranspositionTable(gbytes: ttSize))
		{
		}

		public Solver(TranspositionTable table, MoveLocker locker = null)
		{
			// Parameters
			
			this.table = table;
			this.locker = locker;

			// Preallocate all the memory. Since the algorithm is recursive,
			// we need one list per depth level to make it re-entrant.
			// We'll never need more that 50 depths, right?

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

			var value = ComputeValue(state, depth, 0, -Infinity, Infinity);
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
			if(state.board.BottomWon() || state.board.TopWon())
			{
				return ComputeLeafValue(state);
			}

			if (depth == 0)
			{
				return QuiescenceSearch(state, alpha, beta, 0);
			}

			var startAlpha = alpha;

			// Check transposition table for precalculated value and best move.
			// When using iterative deepening, this should be full for all the states
			// in the principal variations all the way to depth-1 (barring overwrites in the table).
			// This allows us to virtually "resume" the search where we stopped it, visiting almost only
			// the leaves at the deepest level.

			var ttEntry = table.Get(state);

			if (ttEntry.HasValue)
			{
				if (ttEntry.Value.depth >= depth)
				{
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
					{
						return ttEntry.Value.value;
					}
				}
			}

			// Negamax!

			var value = -Infinity;

			var moves = moveLists[depth];
			moves.Clear();

			// Try the best move from the previous deepening iteration first

			bool generatedAllMoves = false;
			int initialMoveCount = 0;

			if (ttEntry.HasValue)
			{
				moves.Add(ttEntry.Value.move);
			}
			else
			{
				state.AddValidMoves(moves);
				generatedAllMoves = true;
				initialMoveCount = moves.Count;
			}

			// Although rare, it can happen that no moves exist for a position
			// In this case, we lose.

			if(moves.Count == 0)
			{
				return -WinScore;
			}

			// Do the thing!

			int bestMoveIndex = -1;
			
			for(int i = 0; i < moves.Count; i++)
			{
				var move = moves[i];

				GameState childState;
				int childValue;

				// In multithread solving, we try to get each thread to work on
				// a different tree. During the first pass through the moves, delay
				// any move that another thread is already working on by pushing
				// it at the end of the queue.

				if (locker?.IsLocked(move,ply) == true && i < initialMoveCount)
				{
					moves.Add(move);
					continue;
				}

				try
				{
					childState = state.ApplyMove(move);
				}
				catch(InvalidMove)
				{
					// There is a very small probability of key collision in the
					// transposition table, in which case we might get an invalid
					// move (saved for another game state than this one).

					Console.WriteLine("Tried applying an invalid move.");

					if (ttEntry.HasValue && ttEntry.Value.move.Equals(move))
						Console.WriteLine("It came from the transposition table.");
					else
						Console.WriteLine("It came from the game state.");

					continue;
				}

				locker?.Lock(move,ply);

				// Principal Variation Search: assume that the first move we try is the best.
				// If it comes from the transposition table (ie. from an earlier deepening iteration),
				// it is quite likely. Since we order moves anyway (win/capture), it is still likely.
				// Fully search the first move, then only try to disprove that it is the best by searching
				// other moves with a "null-window". Only if we find such a refutation, do we search other moves fully.

				if (i == 0)
				{
					childValue = -ComputeValue(childState, depth - 1, ply + 1, -beta, -alpha);
				}
				else
				{
					childValue = -ComputeValue(childState, depth - 1, ply + 1, -alpha-1, -alpha);

					if (childValue > alpha && childValue < beta)
					{
						childValue = -ComputeValue(childState, depth - 1, ply + 1, -beta, -alpha);
					}
				}

				locker?.Unlock(move,ply);

				// Keep track of the best move found so far

				if (childValue > value)
				{
					value = childValue;
					bestMoveIndex = i;
				}

				// Update the lower bound alpha

				if (value > alpha)
				{
					alpha = value;
				}

				// Cutoff if we are already outside of the given window: this state is too good for the
				// other player, so they will never come here.

				if (alpha >= beta)
					break;

				// For threading purposes: gracefully stop the search

				if (interrupt)
					break;

				// If the first move (guessed to be the best) hasn't caused a cutoff, generate the rest of the moves here

				if(!generatedAllMoves)
				{
					state.AddValidMoves(moves);
					generatedAllMoves = true;
					initialMoveCount = moves.Count;
				}
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
			// In Negamax, state values are always in the point of view of the
			// current player, and should be symmetrical: negate to get the other
			// player's point of view.

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
			// Quiescence search works the same as negamax with alpha-beta pruning,
			// but only considers capturing (or winning) moves. The point is to only
			// compute the value of a leaf if it is "quiet", ie. there are no obvious
			// counter captures that would drastically change its value.

			// It helps fight the "horizon effect" of limiting the search to a certain depth,
			// by getting a deeper, more accurate value for the state, in earlier iterations.

			// We don't memoize these, since they are incomplete searches. In practice these
			// searches are quite fast, since they only consider captures.

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

				var childState = state.ApplyMove(m);
				var childValue = -QuiescenceSearch(childState, -beta, -alpha, depth + 1);

				if (childValue > value)
					value = childValue;

				if (value > alpha)
					alpha = value;

				if (alpha >= beta)
					break;
			}

			return value;
		}
	}
}
