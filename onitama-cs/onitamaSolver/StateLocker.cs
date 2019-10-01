namespace Onitama
{
	public class StateLocker
	{
		private ulong[] keys;

		private const int numBits = 10;
		private ulong mask = (1ul << numBits) - 1;

		public StateLocker()
		{
			var numItems = 1ul << numBits;

			keys = new ulong[numItems];
		}

		public void Lock(GameState state)
		{
			var index = state.hash & mask;
			keys[index] = state.hash;
		}

		public bool IsLocked(GameState state)
		{
			var index = state.hash & mask;

			if(keys[index] == state.hash)
				return true;

			return false;
		}

		public void Unlock(GameState state)
		{
			var index = state.hash & mask;

			if (keys[index] == state.hash)
				keys[index] = 0;
		}
	}
}
