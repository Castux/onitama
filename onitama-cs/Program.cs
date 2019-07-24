using System;
using Onitama;

public static class Program
{
	static public void Main()
	{
		var game = GameState.Default();

		var solver = new Solver(10, TimeSpan.FromSeconds(100000), ttSize: 1);
		solver.Start(game);
		solver.Stats.Print();
		
	}
}