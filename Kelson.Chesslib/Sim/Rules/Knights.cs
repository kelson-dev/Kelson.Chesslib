using System;

namespace Kelson.Chesslib.Sim.Rules
{
    public class KnightMoves : BaseMoveRule
    {
        private static readonly (int dr, int df)[] directions = new (int dr, int df)[] 
        {
            (-2, -1), (-1, -2),
            (-2,  1), (-1,  2),
            ( 2, -1), ( 1, -2),
            ( 2,  1), ( 1,  2),
        };

        public override Piece AppliesTo => Piece.Knight;

        public override void EnumerateMoves(Chessboard board, PlayerPosition position, ref Span<PlayerMove> moves)
        {
            int count = 0;
            foreach (var direction in directions)
            {
                if (TryAdd(position, direction, out var to))
                {
                    var owner = board.OwnerOf(to.ToCPos());
                    if (owner is null || owner != position.Player)
                        moves[count++] = new PlayerMove(board, position, to);
                }
            }
            moves = moves[..count];
        }
    }
}
