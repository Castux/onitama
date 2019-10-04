using System;
namespace Onitama
{
	public static class Positioning
	{
		// All these scorings are in the Top point of view
		// A good heuristic for positioning helps ordering the moves better,
		// increasing chances of beta-cutoffs.

		public static int Center(Board board)
		{
			var center = 0b00000_00000_00100_00000_00000;
			var ring1 = 0b00000_00100_01010_00100_00000;
			var ring2 = 0b00100_01010_10001_01010_00100;
			var ring3 = 0b01010_10001_00000_10001_01010;

			var topPieces = board.PlayerPiecesBitboard(Player.Top);
			var topPositioning =
				Board.BitCount(topPieces & center) * 4 +
				Board.BitCount(topPieces & ring1) * 3 +
				Board.BitCount(topPieces & ring2) * 2 +
				Board.BitCount(topPieces & ring3) * 1;

			var bottomPieces = board.PlayerPiecesBitboard(Player.Bottom);
			var bottomPositioning =
				Board.BitCount(bottomPieces & center) * 4 +
				Board.BitCount(bottomPieces & ring1) * 3 +
				Board.BitCount(bottomPieces & ring2) * 2 +
				Board.BitCount(bottomPieces & ring3) * 1;

			return topPositioning - bottomPositioning;
		}

		public static int TotalAdvance(Board board)
		{
			var rowBits = 0b11111;
			var topScore = 0;
			var bottomScore = 0;

			var topPieces = board.PlayerPiecesBitboard(Player.Top);
			var bottomPieces = board.PlayerPiecesBitboard(Player.Bottom);

			for (var i = 1; i <= 5; i++)
			{
				topScore += Board.BitCount(topPieces & rowBits) * i;
				bottomScore += Board.BitCount(bottomPieces & rowBits) * (6 - i);

				rowBits <<= 5;
			}

			return topScore - bottomScore;
		}

		public static int MasterAdvance(Board board)
		{
			var rowBits = 0b11111;
			var topScore = 0;
			var bottomScore = 0;

			var topPieces = board.GetBitboard(Piece.Master, Player.Top);
			var bottomPieces = board.GetBitboard(Piece.Master, Player.Bottom);

			for (var i = 1; i <= 5; i++)
			{
				topScore += Board.BitCount(topPieces & rowBits) * i;
				bottomScore += Board.BitCount(bottomPieces & rowBits) * (6 - i);

				rowBits <<= 5;
			}

			return topScore - bottomScore;
		}
	}
}
