using System;
using System.Collections.Generic;
using System.Threading;

namespace Onitama
{
	public class ThreadedSolver
	{
		private List<Solver> solvers;
		private bool interrupt;

		public ThreadedSolver(int numThreads, double ttSize)
		{
			var table = new TranspositionTable(gbytes: ttSize);
			var locker = new MoveLocker();

			solvers = new List<Solver>();

			for (int i = 0; i < numThreads; i++)
			{
				solvers.Add(new Solver(table, locker));
			}
		}

		public Solver.Result? ComputeValue(GameState state, int depth)
		{
			var start = DateTime.Now;
			var threads = new Thread[solvers.Count];

			Solver.Result? finalResult = null;

			for(int i = 0; i < solvers.Count; i++)
			{
				int index = i;
				var thread = new Thread(() =>
				{
					var result = solvers[index].ComputeValue(state, depth);

					lock (this)
					{
						if (!finalResult.HasValue && result.HasValue)
						{
							finalResult = result;
							Console.WriteLine(depth + ": " + result.Value.bestMove + " " + result.Value.value + " " + (DateTime.Now - start));
						}

						foreach (var solver in solvers)
							solver.Interrupt();
					}
				});

				threads[index] = thread;
				thread.Start();
			}

			foreach (var thread in threads)
				thread.Join();

			return finalResult;
		}

		public Solver.Result? ComputeValueIterative(GameState state, int depth)
		{
			interrupt = false;

			Solver.Result? result = null;

			var start = DateTime.Now;

			for(int i = 1; i <= depth && !interrupt; i++)
			{
				var thisResult = ComputeValue(state, i);
				if (thisResult.HasValue)
				{
					result = thisResult;
					if (Math.Abs(result.Value.value) == Solver.WinScore)
						break;
				}
			}

			Console.WriteLine("Total time: " + (DateTime.Now - start));

			return result;
		}

		public Solver.Result? Run(GameState state, int depth, TimeSpan timeout)
		{
			Solver.Result? result = null;

			var thread = new Thread(() =>
			{
				result = ComputeValueIterative(state, depth);
			});

			thread.Start();
			thread.Join(timeout);

			Interrupt();

			thread.Join();

			return result;
		}

		public void RunInBackground(GameState state)
		{
			var thread = new Thread(() =>
			{
				ComputeValueIterative(state, int.MaxValue);
			});

			thread.Start();
		}

		public void Interrupt()
		{
			foreach (var solver in solvers)
				solver.Interrupt();

			interrupt = true;
		}
	}
}
