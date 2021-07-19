using System;
using Kelson.Chesslib.Sim;

namespace Kelson.Chesslib
{
    public class ChessPieceNotAtExpectedPosition : Exception
    {
        public ChessPieceNotAtExpectedPosition(Piece piece, CPos position) : base($"Expected {piece} to be at {position} but a piece of that type was not there")
        {

        }
    }
}
