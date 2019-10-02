using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onitama
{
	public class ThreadedSolver
	{
		private List<Solver> solvers;

		public ThreadedSolver(int numThreads, double ttSize)
		{
			var table = new TwoTieredTable(gbytes: ttSize);
			var locker = new StateLocker();

			solvers = new List<Solver>();

			for (int i = 0; i < numThreads; i++)
			{
				solvers.Add(new Solver(table, locker));
			}
		}
		/*
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
		}*/
		
		public int ComputeValue(GameState state, int depth, out Move bestMove)
		{
			var tasks = new List<Task>();

			object _lock = new object();
			Move? firstBestMove = null;
			int bestValue = int.MinValue;

			foreach (var solver in solvers)
			{
				var task = new Task(() =>
				{
					var value = solver.ComputeValue(state, depth, out Move thisBestMove);

					lock (_lock)
					{
						if (!firstBestMove.HasValue)
						{
							firstBestMove = thisBestMove;
							bestValue = value;
						}
					}
				});
				tasks.Add(task);
				task.Start();
			}

			var taskArray = tasks.ToArray();

			Task.WaitAny(taskArray);

			foreach (var solver in solvers)
				solver.Interrupt();

			Task.WaitAll(taskArray);

			bestMove = firstBestMove.Value;
			return bestValue;
		}
	}
}
