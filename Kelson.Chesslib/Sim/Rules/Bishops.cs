using System;

namespace Kelson.Chesslib.Sim.Rules
{
    public class BishopMoves : BaseMoveRule
    {
        public override Piece AppliesTo => Piece.Bishop;

        public override void EnumerateMoves(Chessboard board, PlayerPosition position, ref Span<PlayerMove> moves)
        {
            int count = 0;
            Span<PlayerPosition> buffer = stackalloc PlayerPosition[8];
            void HandleDirection(Span<PlayerPosition> buffer, Span<PlayerMove> moves, (int dr, int df) vector)
            {
                var dir = buffer;
                EnumerateDirection(board, position, 0..8, (-1, 0), ref dir);
                for (int i = 0; i < dir.Length; i++)
                    moves[count++] = new PlayerMove(board, position, dir[i]);
            }

            HandleDirection(buffer, moves, (-1,-1));
            HandleDirection(buffer, moves, (-1, 1));
            HandleDirection(buffer, moves, ( 1,-1));
            HandleDirection(buffer, moves, ( 1, 1));

            moves = moves[..count];
        }
    }

    
}
