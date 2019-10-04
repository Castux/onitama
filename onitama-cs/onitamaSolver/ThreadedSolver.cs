using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

		public int ComputeValue(GameState state, int depth, out Move bestMove)
		{
			var start = DateTime.Now;

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

					var tmp = solvers[index].Interrupted ? " (interrupted)" : "";

					Console.WriteLine(depth + ": " + thisBestMove + " " + values[index] + " " + (DateTime.Now - start) + tmp);
				});

				tasks[index] = task;
				task.Start();
			}
			
			var winner = Task.WaitAny(tasks);

			foreach (var solver in solvers)
				solver.Interrupt();

			Task.WaitAll(tasks);

			bestMove = moves[winner];
			return values[winner];
		}

		public int ComputeValueIterative(GameState state, int depth, out Move bestMove)
		{
			interrupt = false;

			Move move = new Move();
			int value = -Solver.Infinity;

			var start = DateTime.Now;

			for(int i = 1; i <= depth && !interrupt; i++)
			{
				value = ComputeValue(state, i, out move);
				if (Math.Abs(value) == Solver.WinScore)
					break;
			}

			Console.WriteLine("Total time: " + (DateTime.Now - start));

			bestMove = move;
			return value;
		}

		public int Run(GameState state, int depth, TimeSpan timeout, out Move bestMove)
		{
			Move result = new Move();
			int value = -Solver.Infinity;

			var task = new Task(() =>
			{
				value = ComputeValueIterative(state, depth, out result);
			});

			task.Start();
			task.Wait(timeout);

			Interrupt();

			task.Wait();

			bestMove = result;
			return value;
		}

		public void RunInBackground(GameState state)
		{
			var task = new Task(() =>
			{
				ComputeValueIterative(state, int.MaxValue, out Move result);
			});

			task.Start();
		}

		public void Interrupt()
		{
			foreach (var solver in solvers)
				solver.Interrupt();

			interrupt = true;
		}
	}
}
