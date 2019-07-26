using System.Collections.Generic;

namespace Onitama
{
	public class MoveSorter
	{
		private static List<List<Move>> lists;

		static MoveSorter()
		{
			lists = new List<List<Move>>();
		}

		public bool GeneratedAllMoves { private set; get; }

		private GameState state;
		private Move? memoMove;
		private bool winAndCaptureOnly;
		private List<Move> tmp;

		public MoveSorter(int depth, GameState state, Move? memoMove, bool winAndCaptureOnly = false)
		{
			this.state = state;
			this.memoMove = memoMove;
			this.winAndCaptureOnly = winAndCaptureOnly;

			GeneratedAllMoves = false;

			while (lists.Count <= depth)
			{
				lists.Add(new List<Move>());
			}

			tmp = lists[depth];
		}

		public IEnumerable<Move> Moves()
		{
			if(memoMove.HasValue)
			{
				yield return memoMove.Value;
			}

			tmp.Clear();
			state.AddValidMoves(tmp);

			GeneratedAllMoves = true;

			for (int i = 0; i < tmp.Count; i++)
			{
				if (tmp[i].quality == (byte)MoveQuality.Win)
					yield return tmp[i];
			}

			for (int i = 0; i < tmp.Count; i++)
			{
				if (tmp[i].quality == (byte)MoveQuality.Capture)
					yield return tmp[i];
			}

			if (winAndCaptureOnly)
				yield break;

			for (int i = 0; i < tmp.Count; i++)
			{
				if (tmp[i].quality == (byte)MoveQuality.Normal)
					yield return tmp[i];
			}
		}
	}
}
