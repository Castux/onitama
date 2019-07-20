using System.Collections.Generic;

namespace Onitama
{
	public enum MoveQuality
	{
		Unknown,
		Win,
		Capture,
		Normal
	}

	public struct Move
	{
		public readonly byte card;
		public readonly byte from;
		public readonly byte to;
		public readonly byte quality;

		public Move(byte card, byte from, byte to, MoveQuality quality = MoveQuality.Unknown)
		{
			this.card = card;
			this.from = from;
			this.to = to;
			this.quality = (byte)quality;
		}

		public override string ToString()
		{
			var res = Card.Definitions[card].Name + " " + CellToCoords(from) + " " + CellToCoords(to);

			var q = (MoveQuality)quality;
			if (q != MoveQuality.Unknown && q != MoveQuality.Normal)
				res += " " + q;

			return res;
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

		public GameState(Board board, CardState cards, Player player)
		{
			Board = board;
			Cards = cards;
			Player = player;
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

		public void ComputeValidMoves(List<Move> outMoves)
		{
			outMoves.Clear();

			if (Board.TopWon() || Board.BottomWon())
				return;

			var ownMaster = Board.GetBitboard(Piece.Master, Player);
			var ownStudents = Board.GetBitboard(Piece.Student, Player);
			var opponentMaster = Board.GetBitboard(Piece.Master, Player.Opponent());
			var opponentStudents = Board.GetBitboard(Piece.Student, Player.Opponent());

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

				if (((ownMaster | ownStudents) & fromBit) == 0)
				{
					continue;
				}

				// Check moves from both cards

				ValidMoves(ownMaster, ownStudents, opponentMaster, opponentStudents, from, card1, outMoves);
				ValidMoves(ownMaster, ownStudents, opponentMaster, opponentStudents, from, card2, outMoves);
			}
		}

		private void ValidMoves(int ownMaster, int ownStudents, int opponentMaster, int opponentStudents, byte from, byte card, List<Move> outMoves)
		{
			var destinations = Card.Definitions[card].destinations[(int)Player, from];
			var goalGate = Player == Player.Top ? Board.BottomGateBits : Board.TopGateBits;

			foreach (var dest in destinations)
			{
				var fromBit = 1 << from;
				var destBit = 1 << dest;

				// As long as we don't have a piece there, it's valid

				if (((ownMaster | ownStudents) & destBit) == 0)
				{
					// Check move quality

					var quality = MoveQuality.Normal;

					if ((opponentMaster & destBit) != 0)
						quality = MoveQuality.Win;
					else if ((opponentStudents & destBit) != 0)
						quality = MoveQuality.Capture;

					// Moving master to the goal is also a win!

					if ((ownMaster & fromBit) != 0 && destBit == goalGate)
					{
						quality = MoveQuality.Win;
					}

					outMoves.Add(new Move(card, from, dest, quality));
				}
			}
		}

		public GameState ApplyMove(Move move)
		{
			return new GameState
			{
				Cards = Cards.Move(move.card),
				Board = Board.Move(move.from, move.to),
				Player = Player.Opponent()
			};
		}
	}
}
