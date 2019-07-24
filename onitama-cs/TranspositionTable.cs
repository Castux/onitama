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

		private struct Entry
		{
			public ulong key;
			public Value value;
		}

		public struct Value
		{
			public Move move;
			public sbyte value;
			public byte depth;
			public Flag flag;
		}

		private uint mask;

		private Entry[] entries;

		public TranspositionTable(int bits)
		{
			uint size = 1u << bits;
			entries = new Entry[size];

			mask = size - 1;
		}

		private void RawAdd(ulong key, Value value)
		{
			var index = key & mask;
			entries[index] = new Entry
			{
				key = key,
				value = value
			};
		}

		private Value? RawGet(ulong key)
		{
			var index = key & mask;

			if (entries[index].key == key)
				return entries[index].value;

			return null;
		}

		public void Add(GameState game, Move move, int value, int depth, Flag flag)
		{
			var entry = new Value
			{
				move = move,
				value = (sbyte)value,
				depth = (byte)depth,
				flag = flag
			};

			RawAdd(game.hash, entry);
		}

		public Value? Get(GameState game)
		{
			return RawGet(game.hash);
		}
	}
}
