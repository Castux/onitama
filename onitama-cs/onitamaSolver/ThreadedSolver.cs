using System;
using System.Collections.Generic;
using System.Threading;

namespace Onitama
{
	public class ThreadedSolver
	{
		private TranspositionTable table;
		private List<Solver> solvers;
		private bool interrupt;

		public ThreadedSolver(int numThreads, double ttSize)
		{
			table = new TranspositionTable(gbytes: ttSize);
			var locker = new MoveLocker();

			solvers = new List<Solver>();

			for (int i = 0; i < numThreads; i++)
			{
				solvers.Add(new Solver(table, locker));
			}
		}

		public void ComputeValue(GameState state, int depth)
		{
			var start = DateTime.Now;
			var threads = new Thread[solvers.Count];

			for(int i = 0; i < solvers.Count; i++)
			{
				int index = i;
				var thread = new Thread(() =>
				{
					solvers[index].ComputeValue(state, depth);

					foreach (var solver in solvers)
							solver.Interrupt();
				});

				threads[index] = thread;
				thread.Start();
			}

			foreach (var thread in threads)
				thread.Join();
		}

		public void ComputeValueIterative(GameState state, int depth)
		{
			interrupt = false;

			var start = DateTime.Now;

			for(int i = 1; i <= depth && !interrupt; i++)
			{
				ComputeValue(state, i);

				if(!interrupt)
				{
					var result = Result(state);
					Console.WriteLine("Depth {0}: {1} (value {2}) {3}", i, result.move, result.value, DateTime.Now - start);

					if (Math.Abs(result.value) == Solver.WinScore)
						break;
				}
			}

			Console.WriteLine("Total time: " + (DateTime.Now - start));
		}

		public void Run(GameState state, int depth, TimeSpan timeout)
		{
			var thread = new Thread(() =>
			{
				ComputeValueIterative(state, depth);
			});

			thread.Start();
			thread.Join(timeout);

			Interrupt();

			thread.Join();
		}

		public TranspositionTable.Value Result(GameState state)
		{
			return table.Get(state).Value;
		}

		private void Interrupt()
		{
			foreach (var solver in solvers)
				solver.Interrupt();

			interrupt = true;
		}
	}
}
