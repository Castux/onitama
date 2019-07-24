using System;
using Onitama;

public static class Program
{
	static public void Main()
	{
		var game = GameState.Default();

		Console.WriteLine(game);

		var solver = new Solver(14, null, ttSize: 1);
		solver.Start(game);	
	}
}