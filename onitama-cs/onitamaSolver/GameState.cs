using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

		public bool Equals(Move m2)
		{
			return card == m2.card && from == m2.from && to == m2.to;
		}

		public override string ToString()
		{
			return ToString(true);
		}

		public string ToString(bool includeQuality)
		{
			var res = Card.Definitions[card].Name + " " + CellToCoords(from) + " " + CellToCoords(to);

			if (includeQuality)
			{
				var q = (MoveQuality)quality;
				if (q != MoveQuality.Unknown && q != MoveQuality.Normal)
					res += " " + q;
			}

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
		public readonly Board board;
		public readonly CardState cards;
		public readonly Player player;
		public readonly ulong hash;

		public static GameState Default()
		{
			return new GameState(Board.InitialBoard(), CardState.Default(), Player.Bottom);
		}

		public GameState(Board board, CardState cards, Player player)
		{
			this.board = board;
			this.cards = cards;
			this.player = player;

			hash = board.hash ^ cards.hash ^ Hash.HashPlayer(player);
		}

		public override string ToString()
		{
			var res = "";

			res += Card.Definitions[cards.topCard1].Name + " " + Card.Definitions[cards.topCard2].Name;
			if (player == Player.Top)
				res += " [" + Card.Definitions[cards.nextCard].Name + "]";

			res += '\n';
			res += board + "\n";

			res += Card.Definitions[cards.bottomCard1].Name + " " + Card.Definitions[cards.bottomCard2].Name;
			if (player == Player.Bottom)
				res += " [" + Card.Definitions[cards.nextCard].Name + "]";

			return res;
		}

		public void AddValidMoves(List<Move> outMoves, bool winAndCaptureOnly = false)
		{
			if (board.TopWon() || board.BottomWon())
				return;

			var ownMaster = board.GetBitboard(Piece.Master, player);
			var ownStudents = board.GetBitboard(Piece.Student, player);
			var opponentMaster = board.GetBitboard(Piece.Master, player.Opponent());
			var opponentStudents = board.GetBitboard(Piece.Student, player.Opponent());

			byte card1, card2;

			if (player == Player.Top)
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
			for (byte from = 0; from < 25; from++, fromBit <<= 1)
			{
				// Skip if we don't have a piece here

				if (((ownMaster | ownStudents) & fromBit) == 0)
				{
					continue;
				}

				// Check moves from both cards

				ValidMoves(outMoves, ownMaster, ownStudents, opponentMaster, opponentStudents, from, card1, winAndCaptureOnly);
				ValidMoves(outMoves, ownMaster, ownStudents, opponentMaster, opponentStudents, from, card2, winAndCaptureOnly);
			}

			// Order moves by quality, in place!

			int currentIndex = 0;

			for (int i = 0; i < outMoves.Count; i++)
			{
				if (outMoves[i].quality == (byte)MoveQuality.Win)
				{
					var tmp = outMoves[currentIndex];
					outMoves[currentIndex] = outMoves[i];
					outMoves[i] = tmp;

					currentIndex++;
				}
			}

			for (int i = currentIndex; i < outMoves.Count; i++)
			{
				if (outMoves[i].quality == (byte)MoveQuality.Capture)
				{
					var tmp = outMoves[currentIndex];
					outMoves[currentIndex] = outMoves[i];
					outMoves[i] = tmp;

					currentIndex++;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ValidMoves(List<Move> outMoves, int ownMaster, int ownStudents, int opponentMaster, int opponentStudents, byte from, byte card, bool winAndCaptureOnly)
		{
			var destinations = Card.Definitions[card].destinations[(int)player, from];
			var goalGate = player == Player.Top ? Board.BottomGateBits : Board.TopGateBits;
			var fromBit = 1 << from;


			for (int i = 0; i < destinations.Length; i++)
			{
				var dest = destinations[i];
				var destBit = 1 << dest;

				// Can't capture self

				if (((ownMaster | ownStudents) & destBit) != 0)
					continue;

				// Check move quality

				var quality = MoveQuality.Normal;

				if ((opponentMaster & destBit) != 0 || (destBit == goalGate && (ownMaster & fromBit) != 0))
					quality = MoveQuality.Win;
				else if ((opponentStudents & destBit) != 0)
					quality = MoveQuality.Capture;

				if (winAndCaptureOnly && quality == MoveQuality.Normal)
					continue;

				var move = new Move(card, from, dest, quality);
				outMoves.Add(move);
			}
		}

		public GameState ApplyMove(Move move)
		{
			return new GameState(
				board.Move(move.from, move.to),
				cards.Move(move.card),
				player.Opponent()
			);
		}
	}
}
