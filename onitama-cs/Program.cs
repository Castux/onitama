using System;
using Onitama;
using System.Collections.Generic;

public static class Program
{
	static public void Main()
	{
		var game = GameState.Default();
		Console.WriteLine(game);

		Console.WriteLine(" ");

		var moves = new List<Move>();
		game.ValidMoves(moves);
		foreach (var move in moves)
			Console.WriteLine(move);

		game = game.ApplyMove(moves[3], out Piece? capture, out byte cardReceived);

		Console.WriteLine(game);
	}
}
