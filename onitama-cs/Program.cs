using System;
using Onitama;

public static class Program
{
	static public void Main()
	{
		var board = Board.FromString("ttTtt ..... ..... ...B. bb.bb");
		var cards = CardState.FromNames("Tiger", "Ox", "Crane", "Rooster", "Horse");

		var game = new GameState(board, cards, Player.Top);

		Console.WriteLine(game);


		while(true)
		{
			var solver = new Solver(15, null, ttSize: 2);
			solver.Start(game);

			var m = solver.PrincipalVariation()[0];

			Console.WriteLine(m);
			game = game.ApplyMove(m);

			Console.WriteLine(game);

			if (game.board.TopWon() || game.board.BottomWon())
				break;
		}
	}
}