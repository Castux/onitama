using System;
using Onitama;
using System.Collections.Generic;
using System.Security.Cryptography;

public static class Program
{
	static public void Main()
	{
		var game = GameState.Default();

		Console.WriteLine(game);
		Console.WriteLine(" ");

		var maxDepth = 13;

		var solver = new Solver(maxDepth, TimeSpan.FromSeconds(300000));
		solver.Start(game);

		Console.WriteLine("Total time: " + (DateTime.Now - solver.StartTime).TotalMilliseconds / 1000f);

		Console.WriteLine("Total nodes visited: " + solver.NodesVisited);
		Console.WriteLine("Of which leaves: " + solver.LeavesVisited * 1f / solver.NodesVisited * 100 + "%");
		Console.WriteLine("Transposition table hits: " + solver.MemHits * 1f / solver.LeavesVisited * 100 + "%");

		Console.WriteLine("Quiescence nodes per leaf: " + solver.QuiescenceNodesVisited * 1f / solver.LeavesVisited);

		Console.WriteLine("Value: " + solver.Value);

		var moves = solver.PrincipalVariation();

		var g = game;
		foreach (var m in moves)
		{
			Console.Write(m);
			Console.Write(" | ");

			g = g.ApplyMove(m);
		}
		if (moves.Count < maxDepth)
			Console.WriteLine("...");
		else
			Console.WriteLine("");

		Console.WriteLine(g);

	}
}