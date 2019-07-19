using System.Collections.Generic;

namespace Onitama
{
	public struct Move
	{
		byte card;
		byte from;
		byte to;

		public Move(byte card, byte from, byte to)
		{
			this.card = card;
			this.from = from;
			this.to = to;
		}
	}

	public struct GameState
	{
		private Board board;
		private CardState cards;
		private Player player;

		public void ValidMoves(List<Move> outMoves)
		{
            outMoves.Clear();

			var pieces = board.PlayerPiecesBitboard(player);
			
			byte card1, card2;

			if(player == Player.Top)
			{
				card1 = cards.topCard1;
				card2 = cards.topCard2;
			}
			else
			{
				card1 = cards.bottomCard1;
				card2 = cards.bottomCard2;
			}

			int fromBit = 1;
			for (byte from = 0 ; from < 25 ; from++, fromBit <<= 1)
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
			var destinations = Card.Definitions[card].destinations[(int)player, from];

			foreach (var destBit in destinations)
			{
				// As long as we don't have a piece there

				if ((pieces & destBit) == 0)
				{
                    outMoves.Add(new Move(card, from, Board.BitToPos(destBit)));
				}
			}
		}
	}
}
