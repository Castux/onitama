using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using Onitama;
using System.Collections.Generic;

public static class Program
{
	static public void Main(string[] args)
	{
		if(args.Length < 2)
		{
			Console.WriteLine("Usage: mono Program.exe <server> <port>");
			return;
		}

		var address = args[0];
		var port = int.Parse(args[1]);

		using var client = new TcpClient(address, port);
		using var reader = new StreamReader(client.GetStream());

		// Who are we?

		var tmp = reader.ReadLine();
		var player = tmp == "You are Top" ? Player.Top : Player.Bottom;

		Console.WriteLine("We are " + player);

		// What are the cards?

		tmp = reader.ReadLine();
		var match = Regex.Match(tmp, @"Cards: (\w+),(\w+),(\w+),(\w+),(\w+)");
		if (!match.Success)
		{
			Console.WriteLine("Failed at: " + tmp);
			return;
		}

		var cardNames = new List<string>();
		for(int i = 1; i < match.Groups.Count; i++)
		{
			cardNames.Add(match.Groups[i].Value);
		}

		Console.WriteLine("Cards are " + string.Join(", ", cardNames));

		// Who starts?

		tmp = reader.ReadLine();
		var startPlayer = tmp == "Top starts" ? Player.Top : Player.Bottom;

		Console.WriteLine("Starting player is " + startPlayer);

		// Setup game

		var board = Board.FromString("ttTtt ..... ..... ..... bbBbb");
		var cards = CardState.FromNames(cardNames[0], cardNames[1], cardNames[2], cardNames[3], cardNames[4]);

		var game = new GameState(board, cards, startPlayer);

		Console.WriteLine(game);
		//var solver = new Solver(1000, ttSize: 8);

		//solver.Start(game);

		//solver.Stats.Print();
	}
}