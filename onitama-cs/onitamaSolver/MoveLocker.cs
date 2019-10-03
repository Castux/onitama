using System;

namespace Onitama
{
	public class MoveLocker
	{
		private byte[,,][] moves;

		public MoveLocker()
		{
			moves = new byte[Card.Definitions.Length,25,25] [];

			for (int i = 0; i < Card.Definitions.Length; i++)
				for (int j = 0; j < 25; j++)
					for(int k = 0; k < 25; k++)
						moves[i,j,k] = new byte[50];		// 50 ply always gonna be enough, right?
		}

		public void Lock(Move m, int ply)
		{
			var array = moves[m.card,m.from,m.to];

			lock(array)
			{
				array[ply]++;
			}
		}

		public void Unlock(Move m, int ply)
		{
			var array = moves[m.card,m.from,m.to];

			lock (array)
			{
				array[ply]--;
			}
		}

		public bool IsLocked(Move m, int ply)
		{
			return moves[m.card,m.from,m.to][ply] > 0;			
		}
	}
}
