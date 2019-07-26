using System.Collections.Generic;

namespace Onitama
{
	public struct MoveSorter
	{
		private static List<List<Move>> allMoves;
		private static List<List<long>> allScores;

		static MoveSorter()
		{
			allMoves = new List<List<Move>>();
			allScores = new List<List<long>>();
		}

		public bool GeneratedAllMoves { private set; get; }

		private GameState state;
		private Move? memoMove;
		private bool generatedMemoMove;
		private bool winAndCaptureOnly;
		private List<Move> moves;
		private List<long> scores;

		public MoveSorter(int ply, GameState state, Move? memoMove, bool winAndCaptureOnly = false)
		{
			this.state = state;
			this.memoMove = memoMove;
			this.winAndCaptureOnly = winAndCaptureOnly;

			generatedMemoMove = false;
			GeneratedAllMoves = false;

			while (allMoves.Count <= ply)
			{
				allMoves.Add(new List<Move>());
				allScores.Add(new List<long>());
			}

			moves = allMoves[ply];
			scores = allScores[ply];
		}

		public int GetNextIndex()
		{
			if(memoMove.HasValue && !generatedMemoMove)
			{
				moves[0] = memoMove.Value;
				generatedMemoMove = true;
				return 0;
			}

			if(!GeneratedAllMoves)
			{
				moves.Clear();
				scores.Clear();

				state.AddValidMoves(moves, winAndCaptureOnly);

				for(int i = 0; i < moves.Count; i++)
				{
					switch ((MoveQuality)moves[i].quality)
					{
						case MoveQuality.Win:
							scores.Add(long.MaxValue);
							break;
						case MoveQuality.Capture:
							scores.Add(long.MaxValue - 1);
							break;
						case MoveQuality.Unknown:
							scores.Add(long.MinValue);
							break;
						default:
							scores.Add(0);
							break;
					}
				}

				GeneratedAllMoves = true;
			}

			long maxScore = long.MinValue;
			int maxIndex = int.MinValue;

			for(int i = 0; i < moves.Count;i++)
			{
				if(scores[i] > maxScore)
				{
					maxScore = scores[i];
					maxIndex = i;
				}
			}

			if(maxIndex >= 0)
			{
				scores[maxIndex] = long.MinValue;
			}

			return maxIndex;
		}

		public Move GetMove(int i)
		{
			return moves[i];
		}
	}
}
