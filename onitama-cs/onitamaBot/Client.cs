using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using Onitama;
using System.Collections.Generic;
using System.Threading;

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
		var msg = reader.ReadLine();

		if(msg == null)
		{
			throw new Exception("Server dropped connection");
		}

		return msg;
	}
}

public class Client
{
	private Server server;
	private GameState game;
	private Player us;
	private TimeSpan timeout;
	private bool lookahead;

	private ThreadedSolver solver;

	public Client(Server server, int timeout, bool lookahead)
	{
		this.timeout = TimeSpan.FromSeconds(timeout - 1.0);
		this.server = server;
		this.lookahead = lookahead;
	}

	public void Setup(int ttsize, int threads)
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

		// Power up the THINK MACHINE

		solver = new ThreadedSolver(threads, ttsize);
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

		var result = solver.Run(game, int.MaxValue, timeout).Value;
		var move = result.bestMove.Value;

		var str = move.ToString(includeQuality: false);

		Console.WriteLine("We are playing: " + str);
		server.Send(str);

		// We should receive the confirmation

		server.Receive();

		// Apply move

		game = game.ApplyMove(move);
	}

	private void WaitForTheirTurn()
	{
		// Run the solver for their side too, to start looking ahead

		Thread thread = null;

		if(lookahead)
			thread = solver.RunInBackground(game);

		var str = server.Receive();

		if(lookahead)
			solver.Interrupt(thread);

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
		string address;
		int port;
		int timeout;
		int ttsize;
		bool lookahead;
		int threads;

		if (args.Length < 6)
		{
			Console.WriteLine("Usage: mono Program.exe <server> <port> <timeout> <ttsize> <lookahead> <threads>");
			Console.WriteLine("Using defaults: 127.0.0.1:8000, 30 seconds, 8 GB transp. table, do lookahead, 4 threads");
			address = "127.0.0.1";
			port = 8000;
			timeout = 30;
			ttsize = 8;
			lookahead = true;
			threads = 4;
		}
		else
		{
			address = args[0];
			port = int.Parse(args[1]);
			timeout = int.Parse(args[2]);
			ttsize = int.Parse(args[3]);
			lookahead = args[4] == "lookahead";
			threads = int.Parse(args[5]);
		}

		try
		{
			var server = new Server(address, port);
			var client = new Client(server, timeout, lookahead);

			client.Setup(ttsize, threads);
			client.Run();
		}
		catch(Exception e)
		{
			Console.WriteLine("Error: " + e.Message);
		}

		Console.ReadLine();
	}
}