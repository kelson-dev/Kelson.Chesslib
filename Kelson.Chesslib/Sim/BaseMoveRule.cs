using System;

namespace Kelson.Chesslib.Sim
{
    public abstract class BaseMoveRule
    {
        /// <summary>
        /// Defines which piece type the move applies to
        /// </summary>
        public abstract Piece AppliesTo { get; }

        public virtual bool IncludeInCastleCheckDetection => true;

        /// <summary>
        /// Fills the moves buffer with possible moves starting at position
        /// Assigns the moves buffer to the range that was assigned to
        /// </summary>
        public abstract void EnumerateMoves(Chessboard board, PlayerPosition position, ref Span<PlayerMove> moves);

        /// <summary>
        /// Indicates if this rule can be used to reach a given position
        /// Fills the moves buffer with valid moves that reach position, and assigns moves to the range that was assigned to
        /// Returns false if there are no moves within the rule that can reach the position
        /// </summary>
        public virtual bool CanReachPosition(Chessboard board, PlayerPosition target, ref Span<PlayerMove> moves)
        {
            Span<PlayerMove> possible = stackalloc PlayerMove[24];
            EnumerateMoves(board, target, ref possible);

            int a = 0;
            for (int i = 0; i < possible.Length; i++)
            {
                if (possible[i].To.IsPositionallyEquivilentTo(target))
                    moves[a++] = possible[i];
            }
            moves = moves[..a];
            return a > 0;
        }

        protected void EnumerateDirection(Chessboard board, PlayerPosition start, Range range, (int dr, int df) vector, ref Span<PlayerPosition> squares, bool fly = false)
        {
            int count = 0;
            var (o, l) = range.GetOffsetAndLength(8);
            for (int i = 0; i < l; i++)
            {
                if (TryAdd(start, (vector.dr * i, vector.df * i), out var to))
                {
                    var owner = fly ? null : board.OwnerOf(to.ToCPos());
                    if (owner == start.Player)
                        break;
                    squares[count++] = to;
                    if (owner is not null) // owned, but not by current player
                        break;
                }
            }
            squares = squares[..count];
        }

        protected static bool TryAdd(PlayerPosition start, (int dr, int df) vector, out PlayerPosition result)
        {
            result = start;
            var (r, f) = (start.Rank + vector.dr, start.File + vector.df);
            if (r >= 0 && r < 8 && f >= 0 && f < 8)
            {
                result = new (start.Player, r, f);
                return true;
            }
            return false;
        }

        protected static bool TryAdd(NormalizedPlayerPosition start, (int dr, int df) vector, out NormalizedPlayerPosition result)
        {
            result = start;
            var (r, f) = (start.Rank + vector.dr, start.File + vector.df);
            if (r >= 0 && r < 8 && f >= 0 && f < 8)
            {
                result = start.Player == Player.One 
                    ? (new(start.Player, r, f)) 
                    : (new(start.Player, 7 - r, f));
                return true;
            }
            return false;
        }
    }
}
