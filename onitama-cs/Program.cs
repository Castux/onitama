using System;
using Onitama;

public static class Program
{
	static public void Main()
	{
		var board = Board.FromString("ttTtt ..... ..... ...B. bb.bb");
		var cards = CardState.FromNames("Tiger", "Ox", "Crane", "Horse", "Rooster");

		var game = new GameState(board, cards, Player.Top);

		Console.WriteLine(game);

		var solver = new Solver(18, null, ttSize: 2);
		solver.Start(game);

		solver.Stats.Print();
	}
}