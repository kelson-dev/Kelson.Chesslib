using Kelson.Chesslib.Sim;

namespace Kelson.Chesslib.Tests.Utilities
{
    public static class Board
    {
        public readonly ref struct BoardPieceLocationAssigner
        {
            public Player Player { get; init; }
            public Piece Piece { get; init; }

            public Chessboard At(CPos location) => Chessboard.FromPieces((Piece, (Player, location)));
        }

        public static BoardPieceLocationAssigner With(Player player, Piece piece) => new() { Player = player, Piece = piece };
    }
}
