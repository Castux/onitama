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

		private Entry[][,] entries;
		private ulong numIndices;

		private const int split = 1024;

		// The transposition table is two-tiered: in one tier, we replace older
		// entries only if the new entry has a higher depth (which therefore has
		// more information and can skip a larger branch of the search).
		// The other tier is replace-always: younger entries have more probability
		// of hitting again soon.
		// In practice we only ever overwrite one entry at a time: either the
		// depth-preferred, or the other one. No need to have duplicates in the table.

		// This is implemented as an array of arrays, to avoid allocating too large
		// chuncks of memory. Also, this is made thread safe by locking on the
		// array being accessed, so having more granularity diminishes the risk of
		// threads waiting on each other.

		public TranspositionTable(double gbytes)
		{
			var numEntries = (ulong)(gbytes * 1024ul * 1024ul * 1024ul / 16ul);
			numIndices = numEntries / 2;

			entries = new Entry[split][,];

			for (var i = 0ul; i < split; i++)
			{
				entries[i] = new Entry[numIndices / split, 2];
			}

			var items = (ulong)entries.Length * (ulong)entries[0].Length;
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

			var index = game.hash % numIndices;
			var array = entries[index % split];

			lock(array)
			{
				var oldEntry = array[index / split, 0];
				var tableIndex = depth > oldEntry.value.depth ? 0 : 1;

				array[index / split, tableIndex] = entry;
			}
		}

		public Value? Get(GameState game)
		{
			var index = game.hash % numIndices;
			var array = entries[index % split];

			lock (array)
			{
				for (int i = 0; i < 2; i++)
				{
					var entry = array[index / split, i];
					if (entry.key == game.hash)
						return entry.value;
				}
			}

			return null;
		}
	}
}
