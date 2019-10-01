using System;

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
		private const int lowBits = 4;
		private const int lowMask = 0b1111;

		public TranspositionTable(double gbytes)
		{
			var numEntries = (ulong)(gbytes * 1024ul * 1024ul * 1024ul / 16ul);
			var numBits = (int)Math.Log(numEntries, 2);

			numEntries = 1ul << numBits;

			ulong lowSize = 1u << lowBits;
			ulong highSize = numEntries / lowSize;

			entries = new Entry[lowSize][];

			for (var i = 0ul; i < lowSize; i++)
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

			var array = entries[index & lowMask];

			lock (array)
			{
				array[index >> lowBits] = new Entry
				{
					key = key,
					value = value
				};
			}
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

		public bool AddIfHigherDepth(GameState game, Move move, int value, int depth, Flag flag)
		{
			var index = game.hash & mask;
			var array = entries[index & lowMask];

			Entry oldEntry;

			lock (array)
			{
				oldEntry = array[index >> lowBits];
			}

			if (depth > oldEntry.value.depth)   // Also erases an empty entry (since depth is initialized to 0)
			{
				Add(game, move, value, depth, flag);
				return true;
			}
			
			return false;
		}

		public Value? Get(GameState game)
		{
			var index = game.hash & mask;
			var array = entries[index & lowMask];

			Entry entry;

			lock (array)
			{
				entry = array[index >> lowBits];
			}

			if (entry.key == game.hash)
				return entry.value;
			
			return null;
		}
	}

	public struct TwoTieredTable
	{
		private readonly TranspositionTable table1;
		private readonly TranspositionTable table2;

		public TwoTieredTable(double gbytes)
		{
			table1 = new TranspositionTable(gbytes / 2.0);
			table2 = new TranspositionTable(gbytes / 2.0);
		}

		public TranspositionTable.Value? Get(GameState g)
		{
			var entry = table1.Get(g);
			if (!entry.HasValue)
				entry = table2.Get(g);

			return entry;
		}

		public void Add(GameState state, Move move, int value, int depth, TranspositionTable.Flag flag)
		{
			if (!table1.AddIfHigherDepth(state, move, value, depth, flag))
			{
				table2.Add(state, move, value, depth, flag);
			}
		}
	}
}
