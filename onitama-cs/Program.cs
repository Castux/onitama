using System;
using Onitama;

public static class Program
{
	static public void Main()
	{
		var game = GameState.Default();

		var solver = new Solver(12, TimeSpan.FromSeconds(100000), ttSize: 0.1);

		while (true)
		{
			Console.WriteLine("=======");
			Console.WriteLine(game);

			if (game.board.TopWon() || game.board.BottomWon())
				break;

			solver.Start(game);

			Console.WriteLine("TT hits: " + solver.MemHits * 100f / solver.NodesVisited + "%");

			Console.WriteLine("Value: " + solver.Value);
			var move = solver.PrincipalVariation()[0];

			Console.WriteLine(move);

			game = game.ApplyMove(move);
		}

		Console.ReadLine();
	}
}