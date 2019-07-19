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

			for (int from = 0; from < 25; from++)
			{
				for (int i = 0; i < Offsets.Length; i++)
				{
					topDestinations[from, i] = Offsets[i].DestinationBit(from, true);
					bottomDestinations[from, i] = Offsets[i].DestinationBit(from, false);
				}
			}
		}
	}

	public struct CardState
	{
		// Indices to the Card.Definitions array

		public readonly byte topCard1;
		public readonly byte topCard2;
		public readonly byte bottomCard1;
		public readonly byte bottomCard2;
		public readonly byte nextCard;

		public CardState(byte tc1, byte tc2, byte bc1, byte bc2, byte nc)
		{
			topCard1 = tc1;
			topCard2 = tc2;
			bottomCard1 = bc1;
			bottomCard2 = bc2;
			nextCard = nc;
		}

		public CardState Play(byte card, out byte receivedCard)
		{
			receivedCard = nextCard;

			if (topCard1 == card)
			{
				return new CardState(nextCard, topCard2, bottomCard1, bottomCard2, topCard1);
			}
			else if (topCard2 == card)
			{
				return new CardState(topCard1, nextCard, bottomCard1, bottomCard2, topCard2);
			}
			else if (bottomCard1 == card)
			{
				return new CardState(topCard1, topCard2, nextCard, bottomCard2, bottomCard1);
			}
			else if (bottomCard2 == card)
			{
				return new CardState(topCard1, topCard2, bottomCard1, nextCard, bottomCard2);
			}
			else
			{
				throw new System.Exception("Invalid card move");
			}
		}
	}
}
