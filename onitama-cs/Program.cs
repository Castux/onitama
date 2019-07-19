using System;
using Onitama;

public static class Program
{
	static public void Main()
	{
		var b = Board.InitialBoard();

		b = b.Move(Player.Top, 2, 21, out Piece? capture);

		Console.WriteLine(b);
		Console.WriteLine(capture.HasValue ? "capture" : "not capture");

		var card = Card.Definitions[3];

		Console.WriteLine();

		GameState g = new GameState();

		var g2 = g;


	}
}
