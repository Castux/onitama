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

		private Entry[][,] entries;
		private const int lowBits = 4;
		private const int lowMask = (1 << lowBits) - 1;

		public TranspositionTable(double gbytes)
		{
			var numEntries = (ulong)(gbytes * 1024ul * 1024ul * 1024ul / 16ul / 2);
			var numBits = (int)Math.Log(numEntries, 2);

			numEntries = 1ul << numBits;

			ulong lowSize = 1u << lowBits;
			ulong highSize = numEntries / lowSize;

			entries = new Entry[lowSize][,];

			for (var i = 0ul; i < lowSize; i++)
			{
				entries[i] = new Entry[highSize,2];
			}

			mask = numEntries - 1;

			ulong items = (ulong)entries.Length * (ulong)entries[0].Length * 2;
			var trueGbytes = items * 16f / 1024 / 1024 / 1024;
			Console.WriteLine("TT size: " + items + " entries (" + trueGbytes + " GB)");
		}

		public void Add(GameState game, Move move, int value, int depth, Flag flag)
		{
			var entry = new Entry
			{
				key = game.hash,
				value = new Value
				{
					move = move,
					value = (sbyte)value,
					depth = (byte)depth,
					flag = flag
				}
			};

			var index = game.hash & mask;
			var array = entries[index & lowMask];

			lock(array)
			{
				var oldEntry = array[index >> lowBits, 0];
				var tableIndex = depth > oldEntry.value.depth ? 0 : 1;

				array[index >> lowBits, tableIndex] = entry;
			}
		}

		public Value? Get(GameState game)
		{
			var index = game.hash & mask;
			var array = entries[index & lowMask];

			lock (array)
			{
				for (int i = 0; i < 2; i++)
				{
					var entry = array[index >> lowBits, i];
					if (entry.key == game.hash)
						return entry.value;
				}
			}

			return null;
		}
	}
}
