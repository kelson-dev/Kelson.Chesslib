using System;

namespace Kelson.Chesslib.Sim.Rules
{
    public class KingAdjacentMoveRule : BaseMoveRule
    {
        public override Piece AppliesTo => Piece.King;

        public override void EnumerateMoves(Chessboard board, PlayerPosition position, ref Span<PlayerMove> moves)
        {
            int count = 0;
            for (int r = -1; r <= 1; r++)
            {
                for (int f = -1; f <= 1; f++)
                {
                    if (r == 0 && f == 0) continue;
                    if (TryAdd(position, (r, f), out var to))
                    {
                        var owner = board.OwnerOf(to.ToCPos());
                        if (owner is null || owner != position.Player)
                            moves[count++] = new PlayerMove(board, position, to);
                    }
                }
            }
            moves = moves[..count];
        }
    }

    public class KingsideCastleRule : BaseMoveRule
    {
        public override bool IncludeInCastleCheckDetection => false;

        public override Piece AppliesTo => Piece.King;

        public override void EnumerateMoves(Chessboard board, PlayerPosition position, ref Span<PlayerMove> moves)
        {
            var backup = moves;
            moves = moves[..0];
            var (kingPos, rookPos, empty1, empty2) = position.Player == Player.One 
                ? (CPos.E1, CPos.H1, CPos.F1, CPos.G1) 
                : (CPos.E8, CPos.H8, CPos.F8, CPos.G8);
            
            // if either piece has moved
            if (board.HasMoved(kingPos) || board.HasMoved(rookPos)) 
                return;
            // if there is a piece in the way
            if (board[empty1] != Piece.None || board[empty2] != Piece.None)
                return;

            var rules = board.Rules;
            Span<PlayerMove> checkMoveBuffer = stackalloc PlayerMove[24];
            for (int i = 0; i < rules.Length; i++)
            {
                var rule = rules[i];
                if (!rule.IncludeInCastleCheckDetection)
                    continue;
                var buffer= checkMoveBuffer;
                if (rule.CanReachPosition(board, (position.Player, empty1), ref buffer))
                    return;
                buffer = checkMoveBuffer;
                if (rule.CanReachPosition(board, (position.Player, empty2), ref buffer))
                    return;
            }

            // Castle allowed
            backup[0] = new PlayerMove(board, position, (position.Player, empty2), isKingsideCastle: true);
            moves = backup[..1];
        }
    }

    public class QueensideCastleRule : BaseMoveRule
    {
        public override bool IncludeInCastleCheckDetection => false;

        public override Piece AppliesTo => Piece.King;

        public override void EnumerateMoves(Chessboard board, PlayerPosition position, ref Span<PlayerMove> moves)
        {
            var backup = moves;
            moves = moves[..0];
            var (kingPos, rookPos, empty1, empty2, empty3) = position.Player == Player.One
                ? (CPos.E1, CPos.A1, CPos.D1, CPos.C1, CPos.B1)
                : (CPos.E8, CPos.A8, CPos.D8, CPos.C8, CPos.B1);

            // if either piece has moved
            if (board.HasMoved(kingPos) || board.HasMoved(rookPos))
                return;
            // if there is a piece in the way
            if (board[empty1] != Piece.None || board[empty2] != Piece.None || board[empty3] != Piece.None)
                return;

            var rules = board.Rules;
            Span<PlayerMove> checkMoveBuffer = stackalloc PlayerMove[24];
            for (int i = 0; i < rules.Length; i++)
            {
                var rule = rules[i];
                if (!rule.IncludeInCastleCheckDetection)
                    continue;
                var buffer = checkMoveBuffer;
                if (rule.CanReachPosition(board, (position.Player, empty1), ref buffer))
                    return;
                buffer = checkMoveBuffer;
                if (rule.CanReachPosition(board, (position.Player, empty2), ref buffer))
                    return;
                buffer = checkMoveBuffer;
                if (rule.CanReachPosition(board, (position.Player, empty3), ref buffer))
                    return;
            }

            // Castle allowed
            backup[0] = new PlayerMove(board, position, (position.Player, empty2), isQueensideCastle: true);
            moves = backup[..1];
        }
    }
}
