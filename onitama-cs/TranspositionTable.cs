using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onitama
{
	public class TranspositionTable
	{
		private uint mask;

		private ulong[] keys;
		private ulong[] values;

		public TranspositionTable(int bits)
		{
			uint size = 1u << bits;
			keys = new ulong[size];
			values = new ulong[size];

			mask = size - 1;

			Console.WriteLine("TT: " + size + "(" + keys.Length + ")");
		}

		private void RawAdd(ulong key, ulong value)
		{
			var index = key & mask;
			keys[index] = key;
			values[index] = value;
		}

		private ulong? RawGet(ulong key)
		{
			var index = key & mask;

			if (keys[index] == key)
				return values[index];

			return null;
		}

		public void Add(GameState game, Move move, int value, int depth)
		{
			var low = move.card << 24 + move.from << 16 + move.to << 8 + value;
			var high = depth;

			ulong packed = ((ulong)high << 32) + (uint)low;

			RawAdd(game.hash, packed);
		}

		public bool Get(GameState game, out Move move, out int value, out int depth)
		{
			var packed = RawGet(game.hash);

			if(packed.HasValue)
			{
				var card = (packed.Value >> 24) & 0xFF;
				var from = (packed.Value >> 16) & 0xFF;
				var to   = (packed.Value >>  8) & 0xFF;

				depth = (int) (packed.Value >> 32) & 0xFF;

				move = new Move((byte)card, (byte)from, (byte)to);
				value = (int) packed.Value & 0xFF;

				return true;
			}
			else
			{
				move = new Move();
				value = -int.MaxValue;
				depth = -1;

				return false;
			}
		}
	}
}
