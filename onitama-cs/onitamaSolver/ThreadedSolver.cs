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

			solvers = new List<Solver>();

			for (int i = 0; i < numThreads; i++)
			{
				solvers.Add(new Solver(table));
			}
		}

		public int ComputeValue(GameState state, int depth, out Move bestMove)
		{
			var tasks = new Task[solvers.Count];
			var moves = new Move[solvers.Count];
			var values = new int[solvers.Count];

			for(int i = 0; i < solvers.Count; i++)
			{
				int index = i;
				var task = new Task(() =>
				{
					values[index] = solvers[index].ComputeValue(state, depth, out Move thisBestMove);
					moves[index] = thisBestMove;
				});

				tasks[index] = task;
				task.Start();
			}

			/*
			var winner = Task.WaitAny(tasks);

			foreach (var solver in solvers)
				solver.Interrupt();
			*/

		   Task.WaitAll(tasks);

			bestMove = moves[0];

			for (int i = 0; i < solvers.Count; i++)
				Console.WriteLine(moves[i] + " " + values[i]);

			return values[0];
		}
	}
}
