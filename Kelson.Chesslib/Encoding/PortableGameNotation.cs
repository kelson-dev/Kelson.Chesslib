using System;
using Kelson.Chesslib.Sim;

namespace Kelson.Chesslib.Encoding
{
    public class PortableGameNotation
    {
        public static (bool parsed, PlayerMove move) TryConsumePgnMove(Chessboard board, ReadOnlySpan<char> text, out ReadOnlySpan<char> remaining)
        {
            var backup = text;
            if (text.Length == 0) // End of input
            {
                remaining = backup;
                return (false, default);
            }
            if (char.IsDigit(text[0])) // Move number, skip "1. ", "2... " etc..
            {
                text = text[1..];
                while (text[0] == '.' || text[0] == ' ')
                    text = text[1..];
            }
            // Piece
            if (char.IsUpper(text[0]))
            {
                // castling
                if (text[0] == 'O')
                {
                    if (text[3] == '-') // O-O-O, queen side castle
                    {
                        remaining = text[5..];
                        if (board.ToMove == Player.One) return (true,
                            new PlayerMove(board,
                                new PlayerPosition(Player.One, CPos.E1),
                                new PlayerPosition(Player.One, CPos.B1),
                                isQueensideCastle: true));
                        else return (true,
                            new PlayerMove(board,
                                new PlayerPosition(Player.Two, CPos.E8),
                                new PlayerPosition(Player.Two, CPos.B8),
                                isQueensideCastle: true));
                    }
                    else // O-O, kingside castle
                    {
                        remaining = text[3..];
                        if (board.ToMove == Player.One) return (true,
                            new PlayerMove(board,
                                new PlayerPosition(Player.One, CPos.E1),
                                new PlayerPosition(Player.One, CPos.G1),
                                isQueensideCastle: true));
                        else return (true,
                            new PlayerMove(board,
                                new PlayerPosition(Player.Two, CPos.E8),
                                new PlayerPosition(Player.Two, CPos.G8),
                                isQueensideCastle: true));
                    }
                }
                else if (text[0] == 'N') // Moving knight
                {
                }
            }
            remaining = backup;
            return (false, default);
        }
    }
}
