using System;
using Onitama;
using System.Collections.Generic;
using System.Security.Cryptography;

public static class Program
{
	static public void Main()
	{
		var cards = new CardState(3,13,0,14,10);
		var game = new GameState(Board.InitialBoard(), cards, Player.Top);

		game = game.ApplyMove(new Move(13, 22, 18));

		//game = GameState.Default();

		Console.WriteLine(game);
		Console.WriteLine(" ");

		var solver = new Solver(game, 12);
		solver.ComputeValue();

		Console.WriteLine("Total leaves visited: " + solver.LeavesVisited);
		Console.WriteLine("Total nodes visited: " + solver.NodesVisited);
		Console.WriteLine("Transposition table hits: " + solver.MemHits);

		Console.WriteLine("Value: " + solver.Value);
		Console.ReadLine();
	}
}
