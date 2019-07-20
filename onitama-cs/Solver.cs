using System;
using System.Collections.Generic;

namespace Onitama
{
	public class Solver
	{
		public const int MaxScore = 100;

		public int LeavesVisited { private set; get; }
		public int NodesVisited { private set; get; }
		public int Value { private set; get; }
		public List<Move> BestMoves { private set; get; }

		private GameState root;
		private int maxDepth;
		private List<List<Move>> moveLists;

		private List<Move> tmpMoves;

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

			tmpMoves = new List<Move>();
		}

		public void ComputeValue()
		{
			Value = ComputeValue(root, maxDepth, -MaxScore, MaxScore);
		}

		private int ComputeValue(GameState state, int depth, int alpha, int beta)
		{
			NodesVisited++;

			// TODO: see if we should pass state as reference? Is that even a thing?
			// Negamax!

			var value = int.MinValue;

			var moves = moveLists[depth];
			moves.Clear();
			state.ComputeValidMoves(moves);

			if (depth == 0 || moves.Count == 0)
				return ComputeLeafValue(state);

			// Check most promising moves first: win, capture, normal

			tmpMoves.Clear();

			foreach(var m in moves)
			{
				if (m.quality == (byte)MoveQuality.Win)
					tmpMoves.Add(m);
			}

			foreach (var m in moves)
			{
				if (m.quality == (byte)MoveQuality.Capture)
					tmpMoves.Add(m);
			}

			foreach (var m in moves)
			{
				if (m.quality == (byte)MoveQuality.Normal)
					tmpMoves.Add(m);
			}

			moves = tmpMoves;
			moveLists[depth] = tmpMoves;

			// Do the thing!

			for (int i = 0; i < moves.Count; i++)
			{
				var move = moves[i];

				var childState = state.ApplyMove(move);
				var childValue = -ComputeValue(childState, depth - 1, -beta, -alpha);

				if(childValue > value)
				{
					value = childValue;
				}

				if(value > alpha)
				{
					alpha = value;
					BestMoves[depth] = move;
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
