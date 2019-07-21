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
		private uint[] values;

		public TranspositionTable(int bits)
		{
			uint size = 1u << bits;
			keys = new ulong[size];
			values = new uint[size];

			mask = size - 1;

			Console.WriteLine("TT: " + size + "(" + keys.Length + ")");
		}

		public void RawAdd(ulong key, uint value)
		{
			var index = key & mask;
			keys[index] = key;
			values[index] = value;
		}

		public uint? RawGet(ulong key)
		{
			var index = key & mask;

			if (keys[index] == key)
				return values[index];

			return null;
		}

		private ulong Hash(GameState game)
		{
			return 0;
		}
	}
}
