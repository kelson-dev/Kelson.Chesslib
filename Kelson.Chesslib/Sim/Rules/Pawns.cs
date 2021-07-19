using System;

namespace Kelson.Chesslib.Sim.Rules
{
    public class PawnSingleAdvanceRule : BaseMoveRule
    {
        public override Piece AppliesTo => Piece.Pawn;

        public override bool IncludeInCastleCheckDetection => false;

        public override void EnumerateMoves(Chessboard board, PlayerPosition position, ref Span<PlayerMove> moves)
        {
            int found = 0;
            if (TryAdd(position, (position.Player == Player.One ? 1 : -1, 0), out var next))
            {
                var normalized = next.Normalize();
                if (board[next.ToCPos()] == Piece.None)
                {
                    if (normalized.Rank < 7) 
                        moves[found++] = new PlayerMove(board, position, next);
                    else if (normalized.Rank == 7) // rank is 0..7, so normalized rank 7 is the promotion rank
                    {
                        moves[found++] = new PlayerMove(board, position, next, promotionChoice: Piece.Queen);
                        moves[found++] = new PlayerMove(board, position, next, promotionChoice: Piece.Rook);
                        moves[found++] = new PlayerMove(board, position, next, promotionChoice: Piece.Bishop);
                        moves[found++] = new PlayerMove(board, position, next, promotionChoice: Piece.Knight);
                    }
                }
            }
            moves = moves[..found];
        }
    }

    public class PawnDoubleAdvanceRule : BaseMoveRule
    {
        public override Piece AppliesTo => Piece.Pawn;

        public override bool IncludeInCastleCheckDetection => false;

        public override void EnumerateMoves(Chessboard board, PlayerPosition position, ref Span<PlayerMove> moves)
        {
            int found = 0;
            var normalized = position.Normalize();
            if (normalized.Rank == 1) // rank is 0..7
            {
                var next = normalized + (2, 0);
                if (board[next] == Piece.None)
                    moves[found++] = new PlayerMove(board, position, next.Denormalize());
            }
            moves = moves[..found];
        }
    }

    public class PawnCaptureRule : BaseMoveRule
    {
        public override Piece AppliesTo => Piece.Pawn;

        public override void EnumerateMoves(Chessboard board, PlayerPosition position, ref Span<PlayerMove> moves)
        {
            int found = 0;
            var normalized = position.Normalize();
            if (TryAdd(normalized, (1, 1), out var right))
            {
                var target = board[right.OfOpponent()];
                if (target != Piece.None)
                    moves[found++] = new PlayerMove(board, position, right.Denormalize(), targetedPiece: target);
            }
            if (TryAdd(normalized, (1, -1), out var left))
            {
                var target = board[left.OfOpponent()];
                if (target != Piece.None)
                    moves[found++] = new PlayerMove(board, position, left.Denormalize(), targetedPiece: target);
            }
            moves = moves[..found];
        }
    }

    public class PawnEnPassantRule : BaseMoveRule
    {
        public override Piece AppliesTo => Piece.Pawn;

        public override bool IncludeInCastleCheckDetection => false;

        public override void EnumerateMoves(Chessboard board, PlayerPosition position, ref Span<PlayerMove> moves)
        {
            int found = 0;
            if (board.LastMove.MovedPiece == Piece.Pawn
                && board.LastMove.To.Rank == position.Rank
                && Math.Abs(board.LastMove.From.Rank - board.LastMove.To.Rank) == 2)
            {
                var lastMoveNorm = board.LastMove.To.Normalize();
                if (TryAdd(lastMoveNorm, (0, -1), out var to))
                {
                    moves[found++] = new PlayerMove(board, position, to.Denormalize().OfOpponent(), targetedPiece: Piece.Pawn);
                }
            }
            moves = moves[..found];
        }
    }
}
