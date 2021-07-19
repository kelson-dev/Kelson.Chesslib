using System.Collections.Immutable;
using Kelson.Common.DataStructures.Sets;
using static Kelson.Chesslib.Encoding.ChessInterchangeString;

namespace Kelson.Chesslib.Sim
{
    public readonly struct PlayerMove
    {
        public PlayerPosition From { get; init; }
        public PlayerPosition To { get; init; }
        public Piece MovedPiece { get; init; }
        public Piece PriorPieceAtDestination { get; init; }
        public Piece TargetedPiece { get; init; }
        public Piece PromotionChoice { get; init; }
        public bool IsEnPassant { get; init; }
        public bool IsKingSideCastle { get; init; }
        public bool IsQueenSideCastle { get; init; }

        public PlayerMove(
            Chessboard board, 
            PlayerPosition from, PlayerPosition to, 
            Piece targetedPiece = Piece.Any, Piece promotionChoice = Piece.None, 
            bool isKingsideCastle = false, bool isQueensideCastle = false)
        {
            From = from;
            To = to;
            MovedPiece = board[from];
            PriorPieceAtDestination = board[to.ToCPos()];
            TargetedPiece = targetedPiece == Piece.Any ? PriorPieceAtDestination : targetedPiece;
            PromotionChoice = promotionChoice;
            IsEnPassant = MovedPiece == Piece.Pawn
                && PriorPieceAtDestination == Piece.None
                && TargetedPiece == Piece.Pawn;
            IsKingSideCastle = isKingsideCastle;
            IsQueenSideCastle = isQueensideCastle;
        }

        public static PlayerMove PiecePlacementMove(Piece piece, PlayerPosition position) => new()
        {
            From = position,
            To = position,
            MovedPiece = Piece.None,
            PriorPieceAtDestination = Piece.None,
            PromotionChoice = piece,
            TargetedPiece = Piece.None,
            IsEnPassant = false,
            IsKingSideCastle = false,
            IsQueenSideCastle = false,
        };

        public static PlayerMove FromInterchangeMoveData(Player player, MoveEncoding encoding) => new()
        {
            From = (player, encoding.From),
            To = (player, encoding.To),
            MovedPiece = Piece.Any,
            TargetedPiece = encoding.Target,
            PromotionChoice = encoding.Promotion
        };

        internal ImmutableArray<Piece> ApplyTo(ImmutableArray<Piece> pieces, ref ImmutableSet64 player1Squares, ref ImmutableSet64 player2Squares)
        {
            var (fi, ti) = (From.Index, To.Index);
            var (myPieces, theirPieces) = From.Player == Player.One
                ? (player1Squares, player2Squares)
                : (player2Squares, player1Squares);
            var moved = pieces
                .SetItem(fi, Piece.None)
                .SetItem(ti, MovedPiece);
            myPieces = myPieces.Remove(fi).Add(ti);
            theirPieces = theirPieces.Remove(fi);

            if (IsKingSideCastle)
            {
                var (rookFrom, rookTo) = (new NormalizedPlayerPosition(From.Player, CPos.H1).Index, new NormalizedPlayerPosition(From.Player, CPos.F1).Index);
                moved = moved
                    .SetItem(rookFrom, Piece.None)
                    .SetItem(rookTo, Piece.Rook);
                myPieces = myPieces.Remove(rookFrom).Add(rookTo);
            }
            else if (IsQueenSideCastle)
            {
                var (rookFrom, rookTo) = (new NormalizedPlayerPosition(From.Player, CPos.A1).Index, new NormalizedPlayerPosition(From.Player, CPos.C1).Index);
                moved = moved
                    .SetItem(fi, Piece.None)
                    .SetItem(ti, Piece.Rook);
                myPieces = myPieces.Remove(rookFrom).Add(rookTo);
            }
            else if (IsEnPassant)
            {
                int behindTo = new NormalizedPlayerPosition(To.Player, To.Normalize().File - 1, To.Rank).Index;
                moved = moved.SetItem(behindTo, Piece.None);
                theirPieces = theirPieces.Remove(behindTo);
            }

            (player1Squares, player2Squares) = From.Player == Player.One
                ? (myPieces, theirPieces)
                : (theirPieces, myPieces);
            return moved;
        }

        public override string ToString()
        {
            string pieceName = MovedPiece == Piece.Knight ? "N" : MovedPiece.ToString()[..1];
            
            if (PromotionChoice != Piece.None)
                return $"{To}{(PromotionChoice == Piece.Knight ? 'N' : PromotionChoice.ToString()[0])}";
            if (TargetedPiece != Piece.None)
                return $"{pieceName}{From}x{TargetedPiece}{To}";
            else if (MovedPiece == Piece.Pawn)
                return $"{To}";
            else
                return $"{pieceName}{From}.{To}";
        }

        public string ToAlgebraicNotation(IImmutableDictionary<CPos, ImmutableArray<PlayerMove>> allMoves)
        {
            string target = To.ToCPos().ToString().ToLowerInvariant();
            if (allMoves.TryGetValue(To.ToCPos(), out var moves))
            {
                if (moves.Length == 1)
                    return target; // only this piece can move to the destination
                else
                {
                    int thisPieceCount = 0; // count the number of pieces of this type that can move to the destination
                    for (int i = 0; i < moves.Length; i++)
                    {
                        if (moves[i].MovedPiece == MovedPiece)
                            thisPieceCount++;
                    }
                    if (thisPieceCount == 1) // only piece of this kind that can move to the destination is this move
                        return $"{MovedPiece.ToAlgebraicName()}{target}";
                    else // multiple pieces of this kind can move to the destination
                        return $"{MovedPiece.ToAlgebraicName()}{From.ToCPos().ToString().ToLowerInvariant()}.{target}";
                }
            }
            else
            {
                // ????
                // implies "allMoves" didn't contain all the moves
                return ToString();
            }
        }

        public bool Equivalent(PlayerMove other) => From.Index == other.From.Index && To.Index == other.To.Index && PromotionChoice == other.PromotionChoice;
    }
}
