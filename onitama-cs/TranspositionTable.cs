using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onitama
{

	public class TranspositionTable
	{
		public enum Flag : byte
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

		private ulong mask;

		private Entry[][] entries;
		private const int lowBits = 2;
		private const int lowMask = 0b11;

		public TranspositionTable(double gbytes)
		{
			var numEntries = (ulong)(gbytes * 1024ul * 1024ul * 1024ul / 16ul);
			var numBits = (int)Math.Log(numEntries, 2);

			numEntries = 1ul << numBits;

			ulong lowSize = 1u << lowBits;
			ulong highSize = numEntries / lowSize;

			entries = new Entry[lowSize][];

			for(var i = 0ul; i < lowSize; i++)
			{
				entries[i] = new Entry[highSize];
			}

			mask = numEntries - 1;

			ulong items = (ulong)entries.Length * (ulong)entries[0].Length;
			var trueGbytes = items * 16f / 1024 / 1024 / 1024;
			Console.WriteLine("TT size: " + items + " entries (" + trueGbytes + " GB)");
		}

		private void RawAdd(ulong key, Value value)
		{
			var index = key & mask;

			entries[index & lowMask][index >> lowBits] = new Entry
			{
				key = key,
				value = value
			};
		}

		private Value? RawGet(ulong key)
		{
			var index = key & mask;
			var entry = entries[index & lowMask][index >> lowBits];

			if (entry.key == key)
				return entry.value;

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
