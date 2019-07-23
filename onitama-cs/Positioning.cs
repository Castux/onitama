using System;
namespace Onitama
{
	public static class Positioning
	{
		// All these scorings are in the Top point of view

		public static int Center(Board board)
		{
			var center = 0b00000_00000_00100_00000_00000;
			var ring1 = 0b00000_01110_01010_01110_00000;
			var ring2 = 0b11111_10001_10001_10001_11111;

			var topPieces = board.PlayerPiecesBitboard(Player.Top);
			var topPositioning =
				Board.BitCount(topPieces & center) * 4 +
				Board.BitCount(topPieces & ring1) * 2 +
				Board.BitCount(topPieces & ring2) * 1;

			var bottomPieces = board.PlayerPiecesBitboard(Player.Bottom);
			var bottomPositioning =
				Board.BitCount(bottomPieces & center) * 4 +
				Board.BitCount(bottomPieces & ring1) * 2 +
				Board.BitCount(bottomPieces & ring2) * 1;

			return topPositioning - bottomPositioning;
		}

		public static int Advance(Board board)
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
	}
}
