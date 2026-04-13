using System;
using System.Linq;

namespace Aya;

public class UciHandler
{
    private Board _board = new();
    private MoveGenerator _generator;
    private Search _search;

    public UciHandler()
    {
        _generator = new MoveGenerator(_board);
        _search = new Search(_board);
    }

    public void Run()
    {
        while (true)
        {
            string? line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string command = parts[0].ToLower();

            switch (command)
            {
                case "uci":
                    HandleUci();
                    break;
                case "isready":
                    Console.WriteLine("readyok");
                    break;
                case "ucinewgame":
                    _board = new Board();
                    _generator = new MoveGenerator(_board);
                    _search = new Search(_board);
                    break;
                case "position":
                    HandlePosition(parts);
                    break;
                case "go":
                    HandleGo(parts);
                    break;
                case "quit":
                    return;
            }
        }
    }

    private void HandleUci()
    {
        Console.WriteLine("id name Aya");
        Console.WriteLine("id author Lucas Campos");
        Console.WriteLine("uciok");
    }

    private void HandlePosition(string[] parts)
    {
        // position [fen <fenstring> | startpos] moves <move1> .... <moveN>
        int movesIndex = Array.IndexOf(parts, "moves");
        
        if (parts[1] == "startpos")
        {
            _board.LoadFromFen(Board.InitialFen);
        }
        else if (parts[1] == "fen")
        {
            // Reconstruct FEN string (parts 2 to movesIndex-1)
            int end = movesIndex == -1 ? parts.Length : movesIndex;
            string fen = string.Join(" ", parts.Skip(2).Take(end - 2));
            _board.LoadFromFen(fen);
        }

        if (movesIndex != -1)
        {
            for (int i = movesIndex + 1; i < parts.Length; i++)
            {
                string uciMove = parts[i];
                var legalMoves = _generator.GenerateLegalMoves().ToList();
                var move = legalMoves.FirstOrDefault(m => m.ToString() == uciMove);
                if (!move.Equals(default(Move)))
                {
                    _board.MakeMove(move);
                }
                else
                {
                    // This is critical: if we miss a move, our internal board is wrong
                    Console.Error.WriteLine($"info string Error: Illegal move received or not recognized: {uciMove}");
                }
            }
        }
    }

    private void HandleGo(string[] parts)
    {
        int depth = 4; // Default depth
        int depthIndex = Array.IndexOf(parts, "depth");
        if (depthIndex != -1 && depthIndex + 1 < parts.Length)
        {
            int.TryParse(parts[depthIndex + 1], out depth);
        }

        int score = _search.StartSearch(depth);
        
        // score is absolute (Positive = White better)
        // UCI expects score relative to side to move
        int uciScore = (_board.ActiveColor == PieceColor.White) ? score : -score;
        
        Console.WriteLine($"info depth {depth} score cp {uciScore} nodes {Math.Max(1, _search.NodesEvaluated)} pv {_search.BestMove}");
        Console.WriteLine($"bestmove {_search.BestMove}");
    }
}
