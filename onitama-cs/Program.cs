using System;
using Onitama;
using System.Collections.Generic;
using System.Security.Cryptography;

public static class Program
{
	static public void Main()
	{
		var cards = new CardState(14,9,7,3,6);
		var game = new GameState(Board.InitialBoard(), cards, Player.Bottom);

		//game = GameState.Default();

		Console.WriteLine(game);
		Console.WriteLine(" ");

		var solver = new Solver(game, 3);
		solver.ComputeValue();

		Console.WriteLine("Total leaves visited: " + solver.LeavesVisited);
		Console.WriteLine("Total nodes visited: " + solver.NodesVisited);

		Console.WriteLine("Value: " + solver.Value);


		ulong foo = 1ul << 63;
		Console.WriteLine(foo + " " + (foo * foo));

		Console.ReadLine();
	}
}
