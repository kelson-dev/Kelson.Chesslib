using System;

namespace Kelson.Chesslib.Sim
{
    [Flags]
    /// <summary>
    /// These flags fully describe a piece at a given location on a board
    /// </summary>
    public enum Piece
    {
        None = 0,
        Queen = 1,
        Rook = 2,
        Knight = 3,
        Bishop = 4,
        Pawn = 5,
        King = 6,
        Any = 7
    }

    public static class PieceExtensions
    {
        public static bool IsFullPiece(this Piece piece) => piece != Piece.None && piece != Piece.Any && piece != Piece.Pawn;

        public static bool TryGetPieceByCharName(this char c, out Piece piece)
        {
            piece = char.ToUpperInvariant(c) switch
            {
                'Q' => Piece.Queen,
                'R' => Piece.Rook,
                'N' => Piece.Knight,
                'B' => Piece.Bishop,
                'P' => Piece.Pawn,
                'K' => Piece.King,
                _ => Piece.None
            };
            return piece != Piece.None;

        }

        public static string ToAlgebraicName(this Piece piece) => piece switch
        {
            Piece.None => "",
            Piece.Queen => "Q",
            Piece.Rook => "R",
            Piece.Knight => "N",
            Piece.Bishop => "B",
            Piece.Pawn => "P",
            Piece.King => "K",
            _ => "?"
        };
    }
}
