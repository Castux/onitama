using System;

namespace Onitama
{
	public enum Piece
	{
		Master,
		Student
	};

	public enum Player
	{
		Top,
		Bottom
	};

	public static class PlayerExtensions
	{
		public static Player Opponent(this Player p)
		{
			if (p == Player.Top)
				return Player.Bottom;
			else
				return Player.Top;
		}
	}

	public struct Board
	{
		// Cell i is represented by (1 << i) in bitboards
		// Cells go top-left to bottom-right, line by line, 0 to 24

		public readonly int topStudents;
		public readonly int bottomStudents;
		public readonly int topMaster;
		public readonly int bottomMaster;

		public const int TopGateBits = 0b00000_00000_00000_00000_00100;
		public const int BottomGateBits = 0b00100_00000_00000_00000_00000;

		public Board(int tS, int bS, int tM, int bM)
		{
			topStudents = tS;
			bottomStudents = bS;
			bottomMaster = bM;
			topMaster = tM;
		}

		public static Board InitialBoard()
		{
			return new Board(
				tS: 0b00000_00000_00000_00000_11011,
				bS: 0b11011_00000_00000_00000_00000,
				tM: TopGateBits,
				bM: BottomGateBits
			);
		}

		public int TopStudentCount()
		{
			return BitCount(topStudents);
		}

		public int BottomStudentCount()
		{
			return BitCount(bottomStudents);
		}

		public bool TopWon()
		{
			return topMaster == BottomGateBits || bottomMaster == 0;
		}

		public bool BottomWon()
		{
			return bottomMaster == TopGateBits || topMaster == 0;
		}

		public Board Move(int from, int to)
		{
			int fromBit = 1 << from;
			int toBit = 1 << to;

			/*
			// Do we capture?

			if ((GetBitboard(Piece.Master, player.Opponent()) & toBit) != 0)
				capture = Piece.Master;
			else if ((GetBitboard(Piece.Student, player.Opponent()) & toBit) != 0)
				capture = Piece.Student;
			else
				capture = null;
			*/

			// Apply move

			return RawMove(fromBit, toBit);
		}

		public override string ToString()
		{
			var res = "  abcde\n";

			for (int i = 0; i < 25; i++)
			{
				if (i % 5 == 0)
					res += (i / 5 + 1) + " ";

				var bit = 1 << i;

				if ((topMaster & bit) != 0)
					res += 'T';
				else if ((topStudents & bit) != 0)
					res += 't';
				else if ((bottomMaster & bit) != 0)
					res += 'B';
				else if ((bottomStudents & bit) != 0)
					res += 'b';
				else
					res += '.';

				if (i % 5 == 4)
					res += '\n';
			}

			return res;
		}

		private static int BitCount(int bitBoard)
		{
			int count = 0;
			while (bitBoard > 0)
			{
				count += bitBoard % 2;
				bitBoard /= 2;
			}

			return count;
		}

		public static byte BitToPos(int bitBoard)
		{
			for (byte i = 0; i < 25; i++)
			{
				if (bitBoard % 2 > 0)
					return i;

				bitBoard >>= 1;
			}

			return 0;
		}

		private int MoveBitBoard(int bitboard, int fromBit, int toBit)
		{
			// Extract bit

			var bit = bitboard & fromBit;

			// Erase it

			bitboard &= ~bit;

			// Insert back in (overwrite)

			if (bit != 0)
			{
				return bitboard | toBit;
			}
			else
			{
				return bitboard & ~toBit;
			}
		}

		private Board RawMove(int fromBit, int toBit)
		{
			return new Board(
				tM: MoveBitBoard(topMaster, fromBit, toBit),
				bM: MoveBitBoard(bottomMaster, fromBit, toBit),
				tS: MoveBitBoard(topStudents, fromBit, toBit),
				bS: MoveBitBoard(bottomStudents, fromBit, toBit)
			);
		}

		public int PlayerPiecesBitboard(Player player)
		{
			return GetBitboard(Piece.Master, player) | GetBitboard(Piece.Student, player);
		}

		public int GetBitboard(Piece piece, Player player)
		{
			if (player == Player.Top)
			{
				if (piece == Piece.Master)
					return topMaster;
				else
					return topStudents;
			}
			else
			{
				if (piece == Piece.Master)
					return bottomMaster;
				else
					return bottomStudents;
			}
		}
	}


}