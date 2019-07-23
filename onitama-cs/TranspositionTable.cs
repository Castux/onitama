using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onitama
{

	public class TranspositionTable
	{
		public enum Flag
		{
			Exact,
			Lower,
			Upper
		}

		public struct Entry
		{
			public Move move;
			public sbyte value;
			public byte depth;
			public Flag flag;
		}

		private uint mask;

		private ulong[] keys;
		private Entry[] values;

		public TranspositionTable(int bits)
		{
			uint size = 1u << bits;
			keys = new ulong[size];
			values = new Entry[size];

			mask = size - 1;

			Console.WriteLine("TT: " + size + "(" + keys.Length + ")");
		}

		private void RawAdd(ulong key, Entry value)
		{
			var index = key & mask;
			keys[index] = key;
			values[index] = value;
		}

		private Entry? RawGet(ulong key)
		{
			var index = key & mask;

			if (keys[index] == key)
				return values[index];

			return null;
		}

		public void Add(GameState game, Move move, int value, int depth, Flag flag)
		{
			var entry = new Entry
			{
				move = move,
				value = (sbyte)value,
				depth = (byte)depth,
				flag = flag
			};

			RawAdd(game.hash, entry);
		}

		public Entry? Get(GameState game)
		{
			return RawGet(game.hash);
		}
	}
}
