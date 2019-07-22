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

			table = new TranspositionTable(24);

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

		private int ComputeValue(GameState state, int depth, int alpha, int beta)
		{
			NodesVisited++;
			if (NodesVisited % 100000 == 0)
				Console.WriteLine("Nodes visited: " + NodesVisited);

			// Check memo

			var memHit = table.Get(state, out Move memMove, out int memValue, out int memDepth);
			if (memHit)
			{
				if (memDepth >= depth)
				{
					MemHits++;
					return memValue;
				}
			}

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

			if(memHit)
			{
				foreach (var m in moves)
					if (m.Equals(memMove))
						tmpMoves.Add(m);
			}

			foreach (var m in moves)
			{
				if (m.quality == (byte)MoveQuality.Win && !m.Equals(memMove))
					tmpMoves.Add(m);
			}

			foreach (var m in moves)
			{
				if (m.quality == (byte)MoveQuality.Capture && !m.Equals(memMove))
					tmpMoves.Add(m);
			}

			foreach (var m in moves)
			{
				if (m.quality == (byte)MoveQuality.Normal && !m.Equals(memMove))
					tmpMoves.Add(m);
			}

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

			table.Add(state, moves[bestMoveIndex], value, depth);

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
