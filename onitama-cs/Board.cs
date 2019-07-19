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

	public struct Board
	{
		// Cell i is represented by (1 << i) in bitboards
		// Cells go top-left to bottom-right, line by line, 0 to 24

		private readonly int topStudents;
		private readonly int bottomStudents;
		private readonly int topMaster;
		private readonly int bottomMaster;

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
				tM: 0b00000_00000_00000_00000_00100,
				bM: 0b00100_00000_00000_00000_00000
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
			return topMaster == 0b00100_00000_00000_00000_00000 || bottomMaster == 0;
		}

		public bool BottomWon()
		{
			return bottomMaster == 0b00000_00000_00000_00000_00100 || topMaster == 0;
		}

		public Board Move(Player player, int from, int to, out Piece? capture)
		{
			int fromBit = 1 << from;
			int toBit = 1 << to;

			// Do we capture?

			if ((GetBitboard(Piece.Master, Opponent(player)) & toBit) != 0)
				capture = Piece.Master;
			else if ((GetBitboard(Piece.Student, Opponent(player)) & toBit) != 0)
				capture = Piece.Student;
			else
				capture = null;

			// Apply move

			return RawMove(fromBit, toBit);
		}

		public override string ToString()
		{
			var chars = new char[25];

			for(int i = 0; i < 25; i++)
			{
				var bit = 1 << i;

				if ((topMaster & bit) != 0)
					chars[i] = 'T';
				else if ((topStudents & bit) != 0)
					chars[i] = 't';
				else if ((bottomMaster & bit) != 0)
					chars[i] = 'B';
				else if ((bottomStudents & bit) != 0)
					chars[i] = 'b';
				else
					chars[i] = '.';
			}

			return new string(chars);
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

		private bool ValidIndex(int i)
		{
			return i >= 0 && i < 25;
		}

		public bool ValidMove(Player player, int from, int to)
		{
			if (!ValidIndex(from) || !ValidIndex(to))
				return false;

			var ownPieces = GetBitboard(Piece.Master, player) | GetBitboard(Piece.Student, player);
			var hasFrom = (ownPieces & (1 << from)) != 0;
			var hasTo = (ownPieces & (1 << to)) != 0;

			return hasFrom && !hasTo;
		}

		private int GetBitboard(Piece piece, Player player)
		{
			if(player == Player.Top)
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

		private Player Opponent(Player p)
		{
			if (p == Player.Top)
				return Player.Bottom;
			else
				return Player.Top;
		}
	}


}