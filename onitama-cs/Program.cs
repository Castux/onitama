using System;
using Onitama;
using System.Collections.Generic;
using System.Security.Cryptography;

public static class Program
{
	static public void Main()
	{
		var cards = new CardState(0,14,3,13,10);
		var game = new GameState(Board.InitialBoard(), cards, Player.Bottom);

		game = game.ApplyMove(new Move(13, 22, 18));

		//game = GameState.Default();

		Console.WriteLine(game);
		Console.WriteLine(" ");

		var solver = new Solver(game, 12);
		solver.ComputeValue();

		Console.WriteLine("Total time: " + (DateTime.Now - solver.StartTime).TotalMilliseconds / 1000f);

		solver = new Solver(game, 12);
		solver.ComputeValueIterative();

		Console.WriteLine("Total time: " + (DateTime.Now - solver.StartTime).TotalMilliseconds / 1000f);



		/*
		Console.WriteLine("Total leaves visited: " + solver.LeavesVisited);
		Console.WriteLine("Total nodes visited: " + solver.NodesVisited);
		Console.WriteLine("Transposition table hits: " + solver.MemHits * 1f / solver.LeavesVisited * 100 + "%");

		Console.WriteLine("Value: " + solver.Value);

		var moves = solver.PrincipalVariation();
		foreach (var m in moves)
		{
			Console.Write(m);
			Console.Write(" | ");
		}
		*/

	}
}
