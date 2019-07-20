using System;
using System.Collections.Generic;

namespace Onitama
{
	public class Solver
	{
		public const int MaxScore = 100;

		public int LeavesVisited { private set; get; }
		public int Value { private set; get; }
		public List<Move> BestMoves { private set; get; }

		private GameState root;
		private int maxDepth;
		private List<List<Move>> moveLists;
		
		public Solver(GameState game, int depth)
		{
			LeavesVisited = 0;
			root = game;
			maxDepth = depth;

			// Preallocate all the move lists
			BestMoves = new List<Move>();
			moveLists = new List<List<Move>>();
			for(int i = 0; i <= depth; i++)
			{
				moveLists.Add(new List<Move>());
				BestMoves.Add(new Move());
			}
		}

		public void ComputeValue()
		{
			Value = ComputeValue(root, maxDepth, -MaxScore, MaxScore);
		}

		private int ComputeValue(GameState state, int depth, int alpha, int beta)
		{
			// TODO: see if we should pass state as reference? Is that even a thing?
			// Negamax!

			var value = int.MinValue;

			var moves = moveLists[depth];
			moves.Clear();
			state.ComputeValidMoves(moves);

			if (depth == 0 || moves.Count == 0)
				return ComputeLeafValue(state);

			for (int i = 0; i < moves.Count; i++)
			{
				var move = moves[i];

				var childState = state.ApplyMove(move);
				var childValue = -ComputeValue(childState, depth - 1, -beta, -alpha);

				if(childValue > value)
				{
					value = childValue;
					BestMoves[depth] = move;
				}

				if(value > alpha)
				{
					alpha = value;
				}

				if (alpha >= beta)
					break;
			}

			return value;
		}

		private int ComputeLeafValue(GameState state)
		{
			LeavesVisited++;
			if (LeavesVisited % 1000000 == 0)
				Console.WriteLine("Leaves visited: " + LeavesVisited);

			if (state.Player == Player.Top)
			{
				if (state.Board.BottomWon())
					return -MaxScore;

				return state.Board.TopStudentCount() - state.Board.BottomStudentCount();
			}

			else
			{
				if (state.Board.TopWon())
					return -MaxScore;

				return state.Board.BottomStudentCount() - state.Board.TopStudentCount();
			}
		}
	}
}
