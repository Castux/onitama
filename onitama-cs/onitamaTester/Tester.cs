using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Onitama;

public static class Tester
{
	public static void Main(string[] args)
	{
		// Arguments parsing

		if(args.Length < 7)
		{
			Console.WriteLine("Usage: mono onitamaTester.exe <board> <cards> <startPlayer> <depth> <timeout> <ttsize> <threads>");
			Exit();
		}

		var boardString = args[0];
		var cards = args[1];
		var startPlayerString = args[2];

		if(boardString == "default")
		{
			boardString = "ttTtt...............bbBbb";
		}

		var board = Board.FromString(boardString);

		var match = Regex.Match(cards, @"(\w+),(\w+),(\w+),(\w+),(\w+)");
		if (!match.Success)
		{
			Console.WriteLine("Failed to parse cards: " + cards);
			Exit();
		}

		var cardNames = new List<string>();
		for (int i = 1; i < match.Groups.Count; i++)
		{
			cardNames.Add(match.Groups[i].Value);
		}

		var cardState = CardState.FromNames(cardNames[0], cardNames[1], cardNames[2], cardNames[3], cardNames[4]);

		var startPlayer = startPlayerString.ToLower() == "top" ? Player.Top : Player.Bottom;

		var gameState = new GameState(board, cardState, startPlayer);

		Console.WriteLine(gameState);

		// Solver arguments

		var depth = int.Parse(args[3]);
		var timeout = TimeSpan.FromSeconds(int.Parse(args[4]));
		var ttsize = int.Parse(args[5]);

		Console.WriteLine("Depth: {0}, timeout: {1}, TT size: {2}", depth, timeout, ttsize);

		// GOGOGO

		var numThreads = int.Parse(args[6]);
		Console.WriteLine("Using {0} threads", numThreads);

		var solver = new ThreadedSolver(numThreads, ttsize);
		solver.Run(gameState, depth, timeout);

		var best = solver.Result(gameState);
		Console.WriteLine("Best move: {0}, value {1}, depth {2})", best.move, best.value, best.depth);

		Exit();
	}

	public static void Exit()
	{
		Console.ReadLine();
		Environment.Exit(0);
	}
}
