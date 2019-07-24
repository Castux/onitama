using System;
using System.Collections.Generic;

namespace Onitama
{
	public class Stats
	{
		private readonly List<PlyStats> statsPerPly = new List<PlyStats>();

		private PlyStats totals = new PlyStats();

		private PlyStats Ply(int ply)
		{
			while (statsPerPly.Count <= ply)
				statsPerPly.Add(new PlyStats());

			return statsPerPly[ply];
		}

		public void NodeVisited(int ply)
		{
			totals.nodesVisited++;
			Ply(ply).nodesVisited++;
		}

		public void LeafVisited(int ply)
		{
			totals.leavesVisited++;
			Ply(ply).leavesVisited++;
		}

		public void QuiescenceNodeVisited()
		{
			totals.quiescenceNodesVisited++;
		}

		public void Recursed(int ply)
		{
			totals.recursed++;
			Ply(ply).recursed++;
		}

		public void MoveExplored(int ply)
		{
			totals.movesExplored++;
			Ply(ply).movesExplored++;
		}

		public void TTLookup(int ply)
		{
			totals.ttLookups++;
			Ply(ply).ttLookups++;
		}

		public void TTHit(int ply)
		{
			totals.ttHits++;
			Ply(ply).ttHits++;
		}

		public void TTCutoff(int ply)
		{
			totals.ttCutoffs++;
			Ply(ply).ttCutoffs++;
		}

		public void BestMoveCutoff(int ply)
		{
			totals.bestMoveCutoffs++;
			Ply(ply).bestMoveCutoffs++;
		}

		public void Print()
		{
			Console.WriteLine("Nodes visited: {0} ({1:f}% leaves)", totals.nodesVisited, totals.leavesVisited * 100.0 / totals.nodesVisited);
			for(var ply = 0; ply < statsPerPly.Count; ply++)
			{
				Console.Write("{0}: {1} ({2:f}% leaves) ", ply, Ply(ply).nodesVisited, Ply(ply).leavesVisited * 100.0 / Ply(ply).nodesVisited);
			}
			Console.WriteLine();

			Console.WriteLine("Quiescence nodes visited: {0} ({1:f} per leaf)", totals.quiescenceNodesVisited, totals.quiescenceNodesVisited * 1.0 / totals.leavesVisited);

			Console.WriteLine("Branching factor: {0:f}", totals.movesExplored * 1.0 / totals.recursed);
			for (var ply = 0; ply < statsPerPly.Count; ply++)
			{
				Console.Write("{0}: {1:f} ", ply, Ply(ply).movesExplored * 1.0 / Ply(ply).recursed);
			}
			Console.WriteLine();

			Console.WriteLine("TT hits: {0:f}%", totals.ttHits * 100.0 / totals.ttLookups);
			for (var ply = 0; ply < statsPerPly.Count; ply++)
			{
				Console.Write("{0}: {1:f}% ", ply, Ply(ply).ttHits * 100.0 / Ply(ply).ttLookups);
			}
			Console.WriteLine();

			Console.WriteLine("TT cutoffs: {0:f}%", totals.ttCutoffs * 100.0 / totals.ttHits);
			for (var ply = 0; ply < statsPerPly.Count; ply++)
			{
				Console.Write("{0}: {1:f}% ", ply, Ply(ply).ttCutoffs * 100.0 / Ply(ply).ttHits);
			}
			Console.WriteLine();

			Console.WriteLine("Best move cutoffs: {0:f}%", totals.bestMoveCutoffs * 100.0 / totals.recursed);
			for (var ply = 0; ply < statsPerPly.Count; ply++)
			{
				Console.Write("{0}: {1:f}% ", ply, Ply(ply).bestMoveCutoffs * 100.0 / Ply(ply).recursed);
			}
			Console.WriteLine();
		}
	}

	public class PlyStats
	{
		public int nodesVisited;
		public int leavesVisited;
		public int quiescenceNodesVisited;

		public int recursed;
		public int movesExplored;

		public int ttLookups;
		public int ttHits;
		public int ttCutoffs;

		public int bestMoveCutoffs;
	}
}
