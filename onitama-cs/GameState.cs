using System.Collections.Generic;

namespace Onitama
{
	public struct Move
	{
		byte card;
		byte from;
		byte to;
	}

	public struct GameState
	{
		private Board board;
		private CardState cards;
		private Player player;

		public IEnumerable<Move> ValidMoves()
		{


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
			for (int from = 0 ; from < 25 ; from++, fromBit >>= 1)
			{
				// Skip if we don't have a piece here

				if ((pieces & fromBit) == 0)
				{
					continue;
				}

				var destinations = Card.Definitions[card1].topDestinations;
			//	foreach(var destBit in Card.Definitions[cards1].)

				fromBit <<= 1;
			}


			yield return new Move();
		}
	}
}
