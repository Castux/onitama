using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using Onitama;
using System.Collections.Generic;

public class Server
{
	private TcpClient client;
	private StreamReader reader;
	private StreamWriter writer;

	public Server(string address, int port)
	{
		client = new TcpClient(address, port);
		reader = new StreamReader(client.GetStream());
		writer = new StreamWriter(client.GetStream());
	}

	public void Send(string s)
	{
		writer.WriteLine(s);
		writer.Flush();
	}

	public string Receive()
	{
		return reader.ReadLine();
	}
}

public class Client
{
	private Server server;
	private GameState game;
	private Player us;
	private int timeout;

	public Client(Server server, int timeout)
	{
		this.timeout = timeout;
		this.server = server;
	}

	public void Setup()
	{
		// Who are we?

		var tmp = server.Receive();
		us = tmp == "You are Top" ? Player.Top : Player.Bottom;

		Console.WriteLine("We are " + us);

		// What are the cards?

		tmp = server.Receive();
		var match = Regex.Match(tmp, @"Cards: (\w+),(\w+),(\w+),(\w+),(\w+)");
		if (!match.Success)
		{
			Console.WriteLine("Failed at: " + tmp);
			return;
		}

		var cardNames = new List<string>();
		for (int i = 1; i < match.Groups.Count; i++)
		{
			cardNames.Add(match.Groups[i].Value);
		}

		Console.WriteLine("Cards are " + string.Join(", ", cardNames));

		// Who starts?

		tmp = server.Receive();
		var startPlayer = tmp == "Top starts" ? Player.Top : Player.Bottom;

		Console.WriteLine("Starting player is " + startPlayer);

		// Setup game

		var board = Board.FromString("ttTtt ..... ..... ..... bbBbb");
		var cards = CardState.FromNames(cardNames[0], cardNames[1], cardNames[2], cardNames[3], cardNames[4]);

		game = new GameState(board, cards, startPlayer);
	}

	public void Run()
	{
		while (true)
		{
			Console.WriteLine("=========");
			Console.WriteLine(game);

			if (game.board.TopWon() || game.board.BottomWon())
			{
				Console.WriteLine("Game over");
				return;
			}

			if (us == game.player)
			{
				PlayOurTurn();
			}
			else
			{
				WaitForTheirTurn();
			}
		}
	}

	private void PlayOurTurn()
	{
		// Our turn

		var moves = new List<Move>();
		game.AddValidMoves(moves);

		// Play the first one :D

		var move = moves[0];
		var str = move.ToString(includeQuality: false);

		Console.WriteLine("We are playing: " + str);
		server.Send(str);

		// We should receive the confirmation

		str = server.Receive();

		// Apply move

		game = game.ApplyMove(move);
	}

	private void WaitForTheirTurn()
	{
		// Their turn

		var str = server.Receive();

		Console.WriteLine("Other player plays: " + str);

		var move = ParseMove(str);
		game = game.ApplyMove(move);
	}

	private static int ColumnNumber(string s)
	{
		switch(s)
		{
			case "a":
				return 0;
			case "b":
				return 1;
			case "c":
				return 2;
			case "d":
				return 3;
			case "e":
				return 4;
			default:
				throw new Exception("Invalid column:" + s);
		}
	}

	private Move ParseMove(string s)
	{
		var match = Regex.Match(s, @"(\w+) (\w)(\d) (\w)(\d)");

		if(!match.Success)
		{
			throw new Exception("Could not parse move: " + s);
		}

		var cardName = match.Groups[1].Value;
		var ocol = ColumnNumber(match.Groups[2].Value);
		var orow = int.Parse(match.Groups[3].Value) - 1;
		var dcol = ColumnNumber(match.Groups[4].Value);
		var drow = int.Parse(match.Groups[5].Value) - 1;

		var card = Card.Index(cardName);
		var origin = orow * 5 + ocol;
		var dest = drow * 5 + dcol;

		return new Move(card, (byte)origin, (byte)dest);
	}
 }

public static class Program
{
	static public void Main(string[] args)
	{
		if(args.Length < 3)
		{
			Console.WriteLine("Usage: mono Program.exe <server> <port> <timeout>");
			return;
		}

		var address = args[0];
		var port = int.Parse(args[1]);
		var timeout = int.Parse(args[2]);

		var server = new Server(address, port);
		var client = new Client(server, timeout);

		client.Setup();
		client.Run();		
	}
}