using System;

namespace Onitama
{
	public static class Hash
	{
		private static Random rnd;

		private static ulong[,,] boardCodes;
		private static ulong[,] cardsCodes;
		private static ulong[] playersCodes;

		static Hash()
		{
			rnd = new Random();

			boardCodes = new ulong[25, 2, 2];

			for (var cell = 0; cell < 25; cell++)
			{
				for (var player = 0; player < 2; player++)
				{
					for (var piece = 0; piece < 2; piece++)
					{
						boardCodes[cell, player, piece] = NextUlong();
					}
				}
			}

			cardsCodes = new ulong[Card.Definitions.Length, 2];

			for (var card = 0; card < Card.Definitions.Length; card++)
			{
				for (var player = 0; player < 2; player++)
				{
					cardsCodes[card, player] = NextUlong();
				}
			}

			playersCodes = new ulong[2];
			for (var player = 0; player < 2; player++)
			{
				playersCodes[player] = NextUlong();
			}
		}

		private static ulong NextUlong()
		{
			return unchecked(((ulong)rnd.Next() << 32) + (ulong)rnd.Next());
		}

		public static ulong HashBoard(Board board)
		{
			var tS = (ulong)board.topStudents;
			var tM = (ulong)board.topMaster;
			var bS = (ulong)board.bottomStudents;
			var bM = (ulong)board.bottomMaster;

			var hash = 0ul;

			for(var cell = 0; cell < 25; cell++)
			{
				hash ^= (tS & 1ul) * boardCodes[cell, (int)Player.Top, (int)Piece.Student];
				hash ^= (tM & 1ul) * boardCodes[cell, (int)Player.Top, (int)Piece.Master];
				hash ^= (bS & 1ul) * boardCodes[cell, (int)Player.Bottom, (int)Piece.Student];
				hash ^= (bM & 1ul) * boardCodes[cell, (int)Player.Bottom, (int)Piece.Master];

				tS >>= 1;
				tM >>= 1;
				bS >>= 1;
				bM >>= 1;
			}

			return hash;
		}

		public static ulong UpdateBoardHash(ulong hash, int cell, Player player, Piece piece)
		{
			return hash ^ boardCodes[cell, (int)player, (int)piece];
		}
	}
}
