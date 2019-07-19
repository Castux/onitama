using System.Collections.Generic;

namespace Onitama
{
	public struct Move
	{
		public readonly byte card;
		public readonly byte from;
		public readonly byte to;

		public Move(byte card, byte from, byte to)
		{
			this.card = card;
			this.from = from;
			this.to = to;
		}

		public override string ToString()
		{
			return Card.Definitions[card].Name + " " + CellToCoords(from) + " " + CellToCoords(to);
		}

		public static string CellToCoords(byte cell)
		{
			var col = cell % 5;
			var row = cell / 5 + 1;

			return "" + (char)((byte)'a' + col) + row;
		}
	}

	public struct GameState
	{
		public Board Board { get; private set; }
		public CardState Cards { get; private set; }
		public Player Player { get; private set; }

		public static GameState Default()
		{
			return new GameState
			{
				Board = Board.InitialBoard(),
				Cards = CardState.Default(),
				Player = Player.Bottom
			};
		}

		public override string ToString()
		{
			var res = "";

			res += Card.Definitions[Cards.topCard1].Name + " " + Card.Definitions[Cards.topCard2].Name;
			if (Player == Player.Top)
				res += " [" + Card.Definitions[Cards.nextCard].Name + "]";

			res += '\n';
			res += Board;

			res += Card.Definitions[Cards.bottomCard1].Name + " " + Card.Definitions[Cards.bottomCard2].Name;
			if (Player == Player.Bottom)
				res += " [" + Card.Definitions[Cards.nextCard].Name + "]";

			return res;
		}

		public void ValidMoves(List<Move> outMoves)
		{
			outMoves.Clear();

			var pieces = Board.PlayerPiecesBitboard(Player);

			byte card1, card2;

			if (Player == Player.Top)
			{
				card1 = Cards.topCard1;
				card2 = Cards.topCard2;
			}
			else
			{
				card1 = Cards.bottomCard1;
				card2 = Cards.bottomCard2;
			}

			int fromBit = 1;
			for (byte from = 0; from < 25; from++, fromBit <<= 1)
			{
				// Skip if we don't have a piece here

				if ((pieces & fromBit) == 0)
				{
					continue;
				}

				// Check moves from both cards

				ValidMoves(pieces, from, card1, outMoves);
				ValidMoves(pieces, from, card2, outMoves);
			}
		}

		private void ValidMoves(int pieces, byte from, byte card, List<Move> outMoves)
		{
			var destinations = Card.Definitions[card].destinations[(int)Player, from];

			foreach (var destBit in destinations)
			{
				// As long as we don't have a piece there

				if ((pieces & destBit) == 0)
				{
					outMoves.Add(new Move(card, from, Board.BitToPos(destBit)));
				}
			}
		}

		public GameState ApplyMove(Move move, out Piece? capture, out byte receivedCard)
		{
			return new GameState
			{
				Cards = Cards.Play(move.card, out receivedCard),
				Board = Board.Move(Player, move.from, move.to, out capture),
				Player = Player.Opponent()
			};
		}
	}
}
