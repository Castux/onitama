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

		private const int split = 16;

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
