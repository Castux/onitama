using System;
using Onitama;
using System.Collections.Generic;
using System.Security.Cryptography;

public static class Program
{
	static public void Main()
	{
		var game = GameState.Default();

		var topSolver = new Solver(1000, TimeSpan.FromSeconds(15));
		var bottomSolver = new Solver(1000, TimeSpan.FromSeconds(6));

		while (true)
		{
			Console.WriteLine("=======");
			Console.WriteLine(game);

			if (game.board.TopWon() || game.board.BottomWon())
				break;

			var solver = (game.player == Player.Top) ? topSolver : bottomSolver;

			solver.Start(game);

			Console.WriteLine("Value: " + solver.Value);
			var move = solver.PrincipalVariation()[0];

			Console.WriteLine(move);

			game = game.ApplyMove(move);
		}
	}
}