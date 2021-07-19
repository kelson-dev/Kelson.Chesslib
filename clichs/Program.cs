using Kelson.Chesslib;
using Kelson.Chesslib.Encoding;
using Kelson.Chesslib.Sim;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static System.Console;

OutputEncoding = Encoding.UTF8;
WriteLine("CLI Chess");
WriteLine("Enter a move in ICCF or Long Algebraic notation to move");
WriteLine("Other commands-");
WriteLine("\t'exit' or 'q' > Exit the chess tool");
WriteLine("\t'player' > Display which players turn it is");
WriteLine("\t'load [cif string]' > Load a board state from a given CIF string");

string? cifString = args.Length == 1 && !args[0].StartsWith("-") ? args[0] : null;

var render = new PlainTextConsoleRenderer(printCifText: true);

var board = Chessboard.StandardStart();
var moves = board.CalculateAllPossibleMoves();

_ = render.Render(in board);
Write("> ");
while (ReadLine() is string line && line != "q" && line != "exit")
{
    var command = line.ToUpperInvariant();
    if (command == "PLAYER")
    {
        WriteLine(board.ToMove);
        Write("> ");
    }
    else if (command.StartsWith("LOAD "))
    {
        string cifText = line[5..];
        try
        {
            var loaded = ChessInterchangeString.Decode(cifText);
            board = loaded;
            moves = board.CalculateAllPossibleMoves();
        }
        catch (Exception ex)
        {
            WriteLine("Could not load cif string");
        }
    }
    else if (command.TryParseChessMove(moves, board, out var move))
    {
        board = board.WithMove(move);
        moves = board.CalculateAllPossibleMoves();
        _ = render.Render(in board);
        Write("> ");
    }
    else
    {
        string movesText = MovesList(moves);
        Write(movesText);
        Write("> ");
    }
}

string MovesList(ImmutableDictionary<CPos, ImmutableArray<PlayerMove>> moves)
{
    var builder = new StringBuilder();
    string line = "";
    foreach (var move in moves.SelectMany(g => g.Value))
    {
        line += move.ToAlgebraicNotation(moves) + ", ";
        if (line.Length > 60)
        {
            builder = builder.AppendLine(line);
            line = "";
        }
    }
    builder = builder.AppendLine(line);
    return builder.ToString();
}