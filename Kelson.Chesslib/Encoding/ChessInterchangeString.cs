using System;
using Kelson.Chesslib.Sim;
using Kelson.Common.DataStructures.Encodings;

namespace Kelson.Chesslib.Encoding
{
    public static class ChessInterchangeString
    {
        public static string Encode(Chessboard board)
        {
            Span<byte> buffer = stackalloc byte[128];
            var encoding = new ChessInterchangeData(board);
            int written = encoding.WriteOut(buffer);
            var data = buffer[..written];
            Span<char> textBuffer = stackalloc char[1024];
            Base58Preview.Encode(data, ref textBuffer);
            return textBuffer.ToString();
        }

        public static Chessboard Decode(string encodedBoard)
        {
            byte[] data = Base58Preview.Decode(encodedBoard);
            if (ChessInterchangeData.TryParse(data, out ChessInterchangeData result))
            {
                return Chessboard.FromChessInterchangeData(result);
            }
            else
            {
                throw new Exception("Could not decode board");
            }
        }

        public unsafe readonly ref struct ChessInterchangeData
        {
            public readonly ChessInterchangeHeader HeaderData;
            public readonly PieceSpanEncoding[] Pieces;
            public readonly MoveEncoding LastMove;
            public readonly MoveEncoding PreviousLastMove;

            public ChessInterchangeData(Chessboard board)
            {
                Span<PieceSpanEncoding> pieceSpans = stackalloc PieceSpanEncoding[64]; // worst case is 64 spans of length 1
                int writtenSpans = 0;
                int spanLength = 1;
                var piece = board[CPos.A1];
                var owner = board.OwnerOf(CPos.A1);
                for (CPos pos = CPos.B1; ((int)pos) < 64; pos = (CPos)((int)pos + 1))
                {
                    var currentPiece = board[pos];
                    var currentOwner = board.OwnerOf(CPos.B1);
                    if (currentPiece == piece && currentOwner == owner)
                        spanLength++;
                    else
                    {
                        pieceSpans[writtenSpans++] = new PieceSpanEncoding(spanLength, owner ?? Player.One, piece);
                        piece = currentPiece;
                        owner = currentOwner;
                    }
                }
                pieceSpans[writtenSpans++] = new PieceSpanEncoding(spanLength, owner ?? Player.One, piece);

                int repetitions = 0; // TODO - count board state repetitions?
                var header = new ChessInterchangeHeader(
                    repetitions, 
                    board.Moves.Length, 
                    board.HasMoved(CPos.A1), board.HasMoved(CPos.E1), board.HasMoved(CPos.H1),
                    board.HasMoved(CPos.A8), board.HasMoved(CPos.E8), board.HasMoved(CPos.H8),
                    writtenSpans);

                MoveEncoding lastMoveEncoding = default;
                MoveEncoding previousLastMovedEncoding = default;
                if (board.Moves.Length > 0)
                {
                    var lastMove = board.Moves[^1];
                    lastMoveEncoding = new MoveEncoding(lastMove.PromotionChoice, lastMove.TargetedPiece, lastMove.From.ToCPos(), lastMove.To.ToCPos());
                }
                if (board.Moves.Length > 1)
                {
                    var lastMove = board.Moves[^2];
                    previousLastMovedEncoding = new MoveEncoding(lastMove.PromotionChoice, lastMove.TargetedPiece, lastMove.From.ToCPos(), lastMove.To.ToCPos());
                }

                HeaderData = header;
                Pieces = pieceSpans[..writtenSpans].ToArray();
                LastMove = lastMoveEncoding;
                PreviousLastMove = previousLastMovedEncoding;
            }

            private ChessInterchangeData(ChessInterchangeHeader header, PieceSpanEncoding[] pieces, MoveEncoding lastMove, MoveEncoding previousLastMoveEncoding)
            {
                HeaderData = header;
                Pieces = pieces;    
                LastMove = lastMove;
                PreviousLastMove = previousLastMoveEncoding;
            }

            public static bool TryParse(ReadOnlySpan<byte> data, out ChessInterchangeData chessData)
            {
                chessData = default;
                if (data.Length < 8)
                    return false;
                if (ChessInterchangeHeader.TryParse(data[..5], out var header))
                {
                    data = data[5..];
                    MoveEncoding m0 = default;
                    MoveEncoding m1 = default;
                    var pieces = new PieceSpanEncoding[header.PieceSpanCount];
                    for (int i = 0; i < header.PieceSpanCount; i++)
                    {
                        bool read = PieceSpanEncoding.TryParse(data, out var span);
                        if (!read)
                            return false;
                        pieces[i] = span;
                        data = data[1..];
                    }
                    if (header.Moves > 0 && MoveEncoding.TryParse(data, out var move))
                    {
                        data = data[3..];
                        m0 = move;
                        if (header.Moves > 1 && MoveEncoding.TryParse(data, out move))
                        {
                            data = data[3..];
                            m1 = move;
                        }
                    }
                    chessData = new ChessInterchangeData(header, pieces, m0, m1);
                    return true;
                }
                return false;
            }

            public int WriteOut(Span<byte> buffer)
            {
                int written = HeaderData.WriteOut(buffer, out var remainingBuffer);
                remainingBuffer[0] = (byte)(Pieces.Length);
                written++;
                remainingBuffer = remainingBuffer[1..];
                for (int i = 0; i < Pieces.Length; i++)
                {
                    written += Pieces[i].WriteOut(remainingBuffer, out remainingBuffer);
                }
                if (HeaderData.Moves > 0)
                    written += LastMove.WriteOut(remainingBuffer, out remainingBuffer);
                if (HeaderData.Moves > 1)
                    written += PreviousLastMove.WriteOut(remainingBuffer, out remainingBuffer);
                return written;
            }
        }

        public readonly ref struct ChessInterchangeHeader
        {
            public readonly byte Version;
            /// <summary>
            /// 3 bits of of repetition count for up to 7 repeated states
            /// 13 bits of move count, for up to 8191 moves
            /// </summary>
            private readonly ushort moveCounterAndStateRepetitions;
            public int Repetitions => moveCounterAndStateRepetitions >> 13;
            public int Moves => moveCounterAndStateRepetitions & 0x1FFF;
            public Player ToMove => (Player)(moveCounterAndStateRepetitions & 1);
            public readonly byte CastleMoveSet;
            public readonly byte PieceSpanCount;

            public bool A1RookUnmoved => (CastleMoveSet & 0b000_00_001) != 0;
            public bool E1KingUnmoved => (CastleMoveSet & 0b000_00_010) != 0; 
            public bool H1RookUnmoved => (CastleMoveSet & 0b000_00_100) != 0; 
            public bool A8RookUnmoved => (CastleMoveSet & 0b001_00_000) != 0; 
            public bool E8KingUnmoved => (CastleMoveSet & 0b010_00_000) != 0;
            public bool H8RookUnmoved => (CastleMoveSet & 0b100_00_000) != 0;

            public ChessInterchangeHeader(int repetitions, int moves, bool a1RookUnmoved, bool e1KingUnmoved, bool h1RookUnmoved, bool a8RookUnmoved, bool e8KingUnmoved, bool h8RookUnmoved, int pieceSpanCount)
            {
                Version = 0x21;
                moveCounterAndStateRepetitions = (ushort)(((repetitions & 0b111) << 13) | (moves & 0b00011111_11111111));
                CastleMoveSet = (byte)(
                      (a1RookUnmoved ? 0b000_00_001 : 0)
                    | (e1KingUnmoved ? 0b000_00_010 : 0)
                    | (h1RookUnmoved ? 0b000_00_100 : 0)
                    | (a8RookUnmoved ? 0b001_00_000 : 0)
                    | (e8KingUnmoved ? 0b010_00_000 : 0)
                    | (h8RookUnmoved ? 0b100_00_000 : 0));
                PieceSpanCount = (byte)(pieceSpanCount);
            }

            private ChessInterchangeHeader(ushort a, byte b, byte c)
            {
                Version = 0x21;
                moveCounterAndStateRepetitions = a;
                CastleMoveSet = b;
                PieceSpanCount = c;
            }

            public static bool TryParse(ReadOnlySpan<byte> data, out ChessInterchangeHeader header)
            {
                header = default;
                if (data.Length < 5)
                    return false;
                if (data[0] != 0x21)
                    return false;
                ushort moveCounterAndStateRepetitions = (ushort)((data[1] << 8) | data[2]);
                header = new ChessInterchangeHeader(moveCounterAndStateRepetitions, data[2], data[3]);
                if (header.PieceSpanCount > 64)
                    return false;
                return true;
            }

            public int WriteOut(Span<byte> buffer, out Span<byte> remainingBuffer)
            {
                buffer[0] = Version;
                buffer[1] = (byte)(moveCounterAndStateRepetitions >> 8);
                buffer[2] = (byte)(moveCounterAndStateRepetitions & 0xFF);
                buffer[3] = CastleMoveSet;
                buffer[4] = PieceSpanCount;
                remainingBuffer = buffer[5..];
                return 5;
            }
        }

        public readonly struct PieceSpanEncoding
        {
            private readonly byte data;
            // 0b1111_1_111
            // count_owner_kind

            public PieceSpanEncoding(int count, Player owner, Piece kind) => data = (byte)(
                ((count & 15) << 4)
              | (((int)owner) << 3)
              | ((int)kind));

            private PieceSpanEncoding(byte data) => this.data = data;

            public int Repetitions => (data & 0b1111_0000) >> 4;
            public Player Owner => (Player)((data & 0b1000) >> 3);
            public Piece Kind => (Piece)(data & 0b0111);

            public static bool TryParse(ReadOnlySpan<byte> data, out PieceSpanEncoding span)
            {
                if (data.Length > 0)
                {
                    span = new(data[0]);
                    return true;
                }

                span = default;
                return false;
            }

            public int WriteOut(Span<byte> buffer, out Span<byte> remainingBuffer)
            {
                buffer[0] = data;
                remainingBuffer = buffer[1..];
                return 1;
            }
        }

        public readonly struct MoveEncoding
        {
            private readonly uint data;
            // 111_111_111111_111_1_00000000_00000000
            // 111_111_111111_111_0_00111111_00000000

            public MoveEncoding(Piece promotion, Piece target, CPos from, CPos to) => data = (uint)(
                ((int)promotion)
              | ((int)target << 3)
              | ((int)to << 6)
              | ((int)from << 18));

            public Piece Promotion => (Piece)(data & 0b111);
            public Piece Target => (Piece)((data >> 3) & 0b111);
            public CPos To => (CPos)((data >> 6) & 0b111111);
            public CPos From => (CPos)((data >> 18) & 0b111111);

            private MoveEncoding(int content) => data = (uint)content;

            public static bool TryParse(ReadOnlySpan<byte> data, out MoveEncoding move)
            {
                move = default;
                if (data.Length < 3)
                    return false;
                int content = data[0] << 16 | data[0] << 8 | data[0];
                move = new(content);
                return true;
            }

            public int WriteOut(Span<byte> buffer, out Span<byte> remainingBuffer)
            {
                buffer[0] = (byte)((data >> 16) & 0xFF);
                buffer[1] = (byte)((data >> 8) & 0xFF);
                buffer[2] = (byte)(data & 0xFF);
                remainingBuffer = buffer[3..];
                return 3;
            }
        }
    }
}
