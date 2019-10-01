using System;
using System.Collections.Generic;
using System.Threading;

namespace Onitama
{
	public class ThreadedSolver
	{
		private List<Solver> solvers;

		public ThreadedSolver(int numThreads, int maxDepth, double ttSize)
		{
			var table = new TwoTieredTable(gbytes: ttSize);
			var locker = new StateLocker();

			solvers = new List<Solver>();

			for (int i = 0; i < numThreads; i++)
			{
				solvers.Add(new Solver(maxDepth, table, locker));
			}
		}

		public void Run(GameState state, TimeSpan timeout)
		{
			var threads = new List<Thread>();

			foreach (var solver in solvers)
			{
				var thread = new Thread(() => solver.Run(state, timeout));
				thread.Start();
				threads.Add(thread);
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}
		}

		public Move BestMove()
		{
			return solvers[0].BestMove();
		}

		public Stats Stats
		{
			get {return solvers[0].Stats;}
		}
	}
}
