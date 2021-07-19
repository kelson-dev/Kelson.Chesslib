using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Kelson.Chesslib.Sim.Rules;
using Kelson.Common.DataStructures.Sets;
using static Kelson.Chesslib.Encoding.ChessInterchangeString;

namespace Kelson.Chesslib.Sim
{
    /// <summary>
    /// Represents a chess board state in memory for calculations to be evaluated against
    /// This type does not provide any sort of "move ranking" like what a chess AI or engine would, it simply allows applying chess rules to a given board state
    /// These operations may include
    /// - Enumerate possible moves for a given piece
    /// - Enumerate moves that end on a given square
    /// - Determine check and checkmate
    /// - Determine if a given move is valid
    /// - Roll forward and roll back moves
    /// </summary>
    public class Chessboard
    {
        private readonly ImmutableArray<Piece> board;
        private readonly Player player;
        private readonly ImmutableSet64 movedPieces;
        private readonly ImmutableSet64 player1Squares;
        private readonly ImmutableSet64 player2Squares;
        public PlayerMove LastMove => Moves.Length > 0 ? Moves[^1] : default;
        public ImmutableArray<PlayerMove> Moves;

        private readonly BaseMoveRule[] ruleSet;
        public ImmutableArray<BaseMoveRule> Rules => ruleSet.ToImmutableArray();

        private static readonly BaseMoveRule[] STANDARD_RULESET = new BaseMoveRule[]
        {
            new PawnSingleAdvanceRule(),
            new PawnDoubleAdvanceRule(),
            new PawnCaptureRule(),
            new PawnEnPassantRule(),
            new QueenMoves(),
            new RookMoves(),
            new BishopMoves(),
            new KnightMoves(),
            new KingAdjacentMoveRule(),
            new KingsideCastleRule(),
            new QueensideCastleRule()
        };

        private Chessboard((Piece, PlayerPosition)[] pieces)
        {
            player = Player.One;
            ruleSet = STANDARD_RULESET;
            var boardBuilder = ImmutableArray<Piece>.Empty.ToBuilder();
            for (int i = 0; i < 64; i++)
                boardBuilder.Add(Piece.None);
            (player1Squares, player1Squares) = (new(), new());
            foreach (var (piece, position) in pieces)
            {
                boardBuilder[position.Index] = piece;
                if (position.Player == Player.One)
                    player1Squares = player1Squares.Add(position.Index);
                else
                    player2Squares = player2Squares.Add(position.Index);
            }
            
            movedPieces = new();
            Moves = ImmutableArray<PlayerMove>.Empty;
            board = boardBuilder.ToImmutable();
        }

        private Chessboard(ReadOnlySpan<(Piece, PlayerPosition)> pieces, ImmutableArray<PlayerMove> moves, ChessInterchangeHeader header)
        {
            player = Player.One;
            ruleSet = STANDARD_RULESET;
            var boardBuilder = ImmutableArray<Piece>.Empty.ToBuilder();
            for (int i = 0; i < 64; i++)
                boardBuilder.Add(Piece.None);
            (player1Squares, player1Squares) = (new(), new());
            foreach (var (piece, position) in pieces)
            {
                boardBuilder[position.Index] = piece;
                if (position.Player == Player.One)
                    player1Squares = player1Squares.Add(position.Index);
                else
                    player2Squares = player2Squares.Add(position.Index);
            }

            movedPieces = new();
            if (header.Version != 0)
            {
                if (!header.A1RookUnmoved)
                    movedPieces = movedPieces.Add((int)CPos.A1);
                if (!header.E1KingUnmoved)
                    movedPieces = movedPieces.Add((int)CPos.E1);
                if (!header.H1RookUnmoved)
                    movedPieces = movedPieces.Add((int)CPos.H1);
                if (!header.A8RookUnmoved)
                    movedPieces = movedPieces.Add((int)CPos.A8);
                if (!header.E8KingUnmoved)
                    movedPieces = movedPieces.Add((int)CPos.E8);
                if (!header.H8RookUnmoved)
                    movedPieces = movedPieces.Add((int)CPos.H8);
            }
            Moves = moves;
            board = boardBuilder.ToImmutable();
        }

        /// <summary>
        /// Strict copy constructor
        /// Does not switch current player or advance the 'last move'
        /// </summary>
        /// <param name="previous"></param>
        public Chessboard(Chessboard previous)
        {
            player = previous.player;
            board = previous.board;
            player1Squares = previous.player1Squares;
            player2Squares = previous.player2Squares;
            ruleSet = previous.ruleSet;
            movedPieces = previous.movedPieces;
            Moves = previous.Moves;
        }

        private Chessboard(Chessboard previous, PlayerMove move)
        {
            player1Squares = previous.player1Squares;
            player2Squares = previous.player2Squares;
            board = move.ApplyTo(previous.board, ref player1Squares, ref player2Squares);
            ruleSet = previous.ruleSet;
            player = previous.player == Player.One ? Player.Two : Player.One;
            if (move.MovedPiece.IsFullPiece() && move.From.Normalize().Rank == 0)
                movedPieces = previous.WithPieceMoved(move.From.ToCPos());
            Moves = previous.Moves.Add(move);
        }

        public Player ToMove => player;

        public Piece this[CPos position] => board[position.ToIndex()];

        /// <summary>
        /// Gets a specific players piece at a given position
        /// If there is no piece or if their is an opponents piece, returns PieceFlags.None
        /// </summary>
        public Piece this[PlayerPosition position]
        {
            get
            {
                int index = position.Index;
                var ownerSet = position.Player == Player.One ? player1Squares : player2Squares;
                if (ownerSet.Contains(index))
                    return board[index];
                else
                    return Piece.None;
            }
        }

        public Piece this[NormalizedPlayerPosition position] => this[position.Denormalize()];

        public Player? OwnerOf(CPos position)
        {
            if (player1Squares.Contains((int)position))
                return Player.One;
            else if (player2Squares.Contains((int)position))
                return Player.Two;
            else
                return null;
        }

        private ImmutableSet64 WithPieceMoved(CPos position) => movedPieces.Add((int)position);

        public bool HasMoved(CPos position) => movedPieces.Contains((int)position);

        /// <summary>
        /// Apply the given move to the board, returning a new board with the updated state
        /// </summary>
        public Chessboard WithMove(PlayerMove move) => new(this, move);

        /// <summary>
        /// Revert a given move from a board, returning a new board representing the state with the move undone
        /// </summary>
        public Chessboard WithoutMove(PlayerMove move) => throw new NotImplementedException();

        public static Chessboard StandardStart() => new(new (Piece, PlayerPosition)[]
        {
            (Piece.Rook,    (Player.One, CPos.A1)), (Piece.Pawn,  (Player.One, CPos.A2)),
            (Piece.Knight,  (Player.One, CPos.B1)), (Piece.Pawn,  (Player.One, CPos.B2)),
            (Piece.Bishop,  (Player.One, CPos.C1)), (Piece.Pawn,  (Player.One, CPos.C2)),
            (Piece.Queen,   (Player.One, CPos.D1)), (Piece.Pawn,  (Player.One, CPos.D2)),
            (Piece.King,    (Player.One, CPos.E1)), (Piece.Pawn,  (Player.One, CPos.E2)),
            (Piece.Bishop,  (Player.One, CPos.F1)), (Piece.Pawn,  (Player.One, CPos.F2)),
            (Piece.Knight,  (Player.One, CPos.G1)), (Piece.Pawn,  (Player.One, CPos.G2)),
            (Piece.Rook,    (Player.One, CPos.H1)), (Piece.Pawn,  (Player.One, CPos.H2)),
            (Piece.Rook,   (Player.Two, CPos.A8)), (Piece.Pawn, (Player.Two, CPos.A7)),
            (Piece.Knight, (Player.Two, CPos.B8)), (Piece.Pawn, (Player.Two, CPos.B7)),
            (Piece.Bishop, (Player.Two, CPos.C8)), (Piece.Pawn, (Player.Two, CPos.C7)),
            (Piece.Queen,  (Player.Two, CPos.D8)), (Piece.Pawn, (Player.Two, CPos.D7)),
            (Piece.King,   (Player.Two, CPos.E8)), (Piece.Pawn, (Player.Two, CPos.E7)),
            (Piece.Bishop, (Player.Two, CPos.F8)), (Piece.Pawn, (Player.Two, CPos.F7)),
            (Piece.Knight, (Player.Two, CPos.G8)), (Piece.Pawn, (Player.Two, CPos.G7)),
            (Piece.Rook,   (Player.Two, CPos.H8)), (Piece.Pawn, (Player.Two, CPos.H7)),
        });

        public static Chessboard FromPieces(params (Piece, PlayerPosition)[] positions) => new(positions);

        public ImmutableDictionary<CPos, ImmutableArray<PlayerMove>> CalculateAllPossibleMoves()
        {
            var results = new List<PlayerMove>();
            Span<PlayerMove> movesBuffer = stackalloc PlayerMove[24];
            for (var pos = CPos.A1; (int)pos < 64; pos = (CPos)((int)pos + 1))
            {
                var piece = board[(int)pos];
                if (piece != Piece.None && OwnerOf(pos) == ToMove)
                {
                    for (int ruleId = 0; ruleId < ruleSet.Length; ruleId++)
                    {
                        var rule = ruleSet[ruleId];
                        if (rule.AppliesTo == piece)
                        {
                            var moves = movesBuffer;
                            rule.EnumerateMoves(this, (ToMove, pos), ref moves);
                            for (int i = 0; i < moves.Length; i++)
                            {
                                var move = moves[i];
                                var previousToDestination = results.Where(r => r.To.Index == move.To.Index).ToArray();
                                if (previousToDestination.Length > 0)
                                {
                                    bool foundDuplicate = false;
                                    for (int p = 0; p < previousToDestination.Length && !foundDuplicate; p++)
                                    {
                                        if (previousToDestination[p].From.Index == move.From.Index)
                                            foundDuplicate = true;
                                    }
                                    if (!foundDuplicate)
                                        results.Add(move);

                                }
                                else
                                    results.Add(move);
                            }
                        }
                    }
                }
            }

            return results.GroupBy(m => m.To.ToCPos())
                .ToImmutableDictionary(
                    g => g.Key, 
                    g => g.ToImmutableArray());
        }

        public static Chessboard FromChessInterchangeData(in ChessInterchangeData data)
        {
            // construct piece array
            int piecesWritten = 0;
            Span<(Piece, PlayerPosition)> pieces = stackalloc (Piece, PlayerPosition)[64];

            int index = 0;
            for (int i = 0; i < data.Pieces.Length; i++)
            {
                var span = data.Pieces[i];
                if (span.Kind != Piece.None)
                {
                    for (int j = 0; j < span.Repetitions; j++)
                        pieces[piecesWritten++] = (span.Kind, (span.Owner, ((CPos)(index + j))));
                }

                index += span.Repetitions;
            }

            var movesBuilder = ImmutableArray<PlayerMove>.Empty.ToBuilder();
            int moveCount = data.HeaderData.Moves;
            var toMove = data.HeaderData.ToMove;
            for (int i = 0; i < moveCount; i++)
            {
                if (i == moveCount - 1)
                    movesBuilder[i] = PlayerMove.FromInterchangeMoveData(toMove.Other(), data.LastMove);
                else if (i == moveCount - 2)
                    movesBuilder[i] = PlayerMove.FromInterchangeMoveData(toMove, data.PreviousLastMove);
                else
                    movesBuilder[i] = default;
            }

            return new Chessboard(pieces[..piecesWritten], moves: movesBuilder.ToImmutable(), header: data.HeaderData);
        }
    }
}
