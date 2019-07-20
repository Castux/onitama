using System;
using Onitama;
using System.Collections.Generic;

public static class Program
{
	static public void Main()
	{
		var board = new Board(
			tS: 0b00000_00000_00000_00000_11011,
			bS: 0b11011_00000_00000_00000_00000,
			tM: 0b00000_00000_00000_00000_00100,
			bM: 0b00000_00000_00100_00000_00000
		);

		var cards = new CardState(6,7,0,1,2);
		var game = new GameState(board, cards, Player.Top);

		//game = GameState.Default();

		Console.WriteLine(game);
		Console.WriteLine(" ");

		var solver = new Solver(game, 12);
		solver.ComputeValue();

		Console.WriteLine("Total leaves visited: " + solver.LeavesVisited);

		Console.WriteLine("Value: " + solver.Value);

		foreach(var m in solver.BestMoves)
			Console.WriteLine(m);

		Console.ReadLine();
	}
}
