using Kelson.Chesslib;
using Kelson.Chesslib.Sim;
using System;
using System.Linq;

class ColorAsciiRenderer : IChessRenderer<Unit>
{
    const ConsoleColor WhitePieces = ConsoleColor.White;
    const ConsoleColor BlackPieces = ConsoleColor.White;
    const ConsoleColor WhiteSquares = ConsoleColor.DarkGray;
    const ConsoleColor BlackSquares = ConsoleColor.DarkRed;

    string PieceName(Piece flag)
    {
        if (flag == Piece.None) return " ";
        return flag switch
        {
            Piece.Queen => "Q",
            Piece.Rook => "R",
            Piece.Knight => "N",
            Piece.Bishop => "B",
            Piece.Pawn => "P",
            Piece.King => "K",
            _ => "?"
        };
    }

    public Unit Render(in Chessboard board)
    {
        //Console.Clear();
        var _bg = Console.BackgroundColor;
        var _fg = Console.ForegroundColor;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;

        Console.Write(' ');
        for (int file = 0; file < 8; file++)
            Console.Write((char)('A' + file));
        Console.Write(' ');
        Console.WriteLine();
        for (int rank = 7; rank >= 0; rank--)
        {
            Console.Write((char)('1' + rank));
            for (int file = 0; file < 8; file++)
            {
                int index = (rank  << 3) + file;
                var pos = (CPos)(index);
                Console.BackgroundColor = pos.IsWhiteSquare() ? WhiteSquares : BlackSquares;
                var piece = board[pos];
                if (piece != Piece.None)
                {
                    if (board[(Player.One, pos)] == piece)
                    {
                        Console.ForegroundColor = WhitePieces;
                        Console.Write(PieceName(piece));
                    }
                    else if (board[(Player.Two, pos)] == piece)
                    {
                        Console.ForegroundColor = BlackPieces;
                        Console.Write(PieceName(piece).ToLowerInvariant());
                    }
                }
                else
                {
                    Console.Write(" ");
                }
            }
            Console.BackgroundColor = _bg;
            Console.ForegroundColor = _fg;
                
            Console.WriteLine();
        }

        Console.Write(' ');
        for (int file = 0; file < 8; file++)
            Console.Write((char)('A' + file));
        Console.Write(' ');

        Console.ForegroundColor = _fg;
        Console.BackgroundColor = _bg;
        Console.WriteLine();
        var moves = board.CalculateAllPossibleMoves();
        string line = "";
        foreach (var move in moves.SelectMany(g => g.Value))
        {
            line += move.ToAlgebraicNotation(moves) + ", ";
            if (line.Length > 60)
            {
                Console.WriteLine(line);
                line = "";
            }
        }
        Console.WriteLine(line);
        Console.Write("Move: ");
        return default;
    }
}

