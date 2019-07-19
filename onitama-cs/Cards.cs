namespace Onitama
{
	public struct Offset
	{
		public readonly int dcol;
		public readonly int drow;

		public Offset(int row, int col)
		{
			dcol = col;
			drow = row;
		}

		public bool ValidIndex(int row, int col)
		{
			return row >= 0 && row < 5 && col >= 0 && col < 5;
		}

		public int DestinationBit(int from, bool topPlayer)
		{
			var row = from / 5;
			var col = from % 5;

			row += drow * (topPlayer ? -1 : 1);
			col += dcol * (topPlayer ? -1 : 1);

			if (!ValidIndex(row, col))
				return 0;

			var to = row * 5 + col;
			return 1 << to;
		}
	}

	public class Card
	{
		public static Card[] Definitions;

		static Card()
		{
			Definitions = new Card[]
			{
				new Card("Tiger", new int[]{ -2,0, 1,0 } ),
				new Card("Crab", new int[]{ -1,0, 0,-2, 0,2 } ),
				new Card("Monkey", new int[]{ -1,-1, -1,1, 1,-1, 1,1 } ),
				new Card("Crane", new int[]{ -1,0, 1,-1, 1,1 } ),
				new Card("Dragon", new int[]{ -1,-2, -1,2, 1,-1, 1,1 } ),
				new Card("Elephant", new int[]{ -1,-1, -1,1, 0,-1, 0,1 } ),
				new Card("Mantis", new int[]{ -1,-1, -1,1, 0,1 } ),
				new Card("Boar", new int[]{ -1,0, 0,-1, 0,1 } ),
				new Card("Frog", new int[]{ -1,-1, 0,-2, 1,1 } ),
				new Card("Goose", new int[]{ -1,-1, 0,-1, 0,1, 1,1 } ),
				new Card("Horse", new int[]{ -1,0, 0,-1, 1,0 } ),
				new Card("Eel", new int[]{ -1,-1, 0,1, 1,-1 } ),
				new Card("Rabbit", new int[]{ -1,1, 0,2, 1,-1 } ),
				new Card("Rooster", new int[]{ -1,1, 0,-1, 0,1, 1,-1 } ),
				new Card("Ox", new int[]{ -1,0, 0,1, 1,0 } ),
				new Card("Cobra", new int[]{ -1,1, 0,-1, 1,1 } )
			};
		}

		public string Name;
		public Offset[] Offsets;

		// Precomputed, indexed by origin cell
		public int[,] topDestinations;
		public int[,] bottomDestinations;

		public Card(string name, int[] offsets)
		{
			Name = name;
			Offsets = new Offset[offsets.Length / 2];


			for (int i = 0; i < offsets.Length; i += 2)
			{
				var offset = new Offset(offsets[i], offsets[i + 1]);
				Offsets[i / 2] = offset;
			}


			topDestinations = new int[25, Offsets.Length];
			bottomDestinations = new int[25, Offsets.Length];

			for(int from = 0; from < 25; from++)
			{
				for(int i = 0; i < Offsets.Length; i++)
				{
					topDestinations[from, i] = Offsets[i].DestinationBit(from, true);
					bottomDestinations[from, i] = Offsets[i].DestinationBit(from, false);
				}
			}
		}
	}


}
