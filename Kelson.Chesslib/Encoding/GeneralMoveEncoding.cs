using System;
using System.Collections.Immutable;
using System.Linq;
using Kelson.Chesslib.Sim;

namespace Kelson.Chesslib.Encoding
{
    public static class GeneralMoveEncoding
    {
        public static bool TryParseChessMove(this string line, ImmutableDictionary<CPos, ImmutableArray<PlayerMove>> moves, Chessboard board, out PlayerMove move)
        {
            line = line.Trim();
            // iccf
            if ((line.Length == 4 || line.Length == 5) && char.IsDigit(line[0]))
            {
                if (int.TryParse(line[..1], out int fromRank) && int.TryParse(line[1..2], out int fromFile) && int.TryParse(line[2..3], out int toRank) && int.TryParse(line[3..4], out int toFile))
                {
                    var from = (CPos)((fromRank & 0b111) << 3) + (fromFile & 0b111);
                    var to = (CPos)((toRank & 0b111) << 3) + (toFile & 0b111);
                    var movesFromFromToTo = moves[to].Where(m => m.From.ToCPos() == from && m.To.ToCPos() == to).ToArray();
                    if (movesFromFromToTo.Length > 1 && line.Length == 5)
                    {
                        if (int.TryParse(line[4..5], out int promoId))
                        {
                            for (int i = 0; i < movesFromFromToTo.Length; i++)
                            {
                                if (movesFromFromToTo[i].PromotionChoice == (Piece)promoId)
                                {
                                    move = movesFromFromToTo[i];
                                    return true;
                                }
                            }
                        }
                    }
                    else if (movesFromFromToTo.Length == 1)
                    {
                        move = movesFromFromToTo[0];
                        return true;
                    }
                }
            }

            // long algebraic 
            if (line.Length >= 2)
            {
                bool pieceSpecified = line[0].TryGetPieceByCharName(out var p);
                line = pieceSpecified ? line[1..] : line;

                if (Enum.TryParse<CPos>(line[..2], out var firstPos))
                {
                    if (line.Length == 2 && moves.TryGetValue(firstPos, out var movesToFirstPos) && movesToFirstPos.Length == 1)
                    {
                        if (movesToFirstPos.Length == 1)
                        {
                            move = movesToFirstPos[0];
                            return true;
                        }
                        else if (pieceSpecified)
                        {
                            int countToFirstPosWithSpecifiedPiece = 0;
                            PlayerMove found = default;
                            for (int i = 0; i < movesToFirstPos.Length; i++)
                            {
                                if (movesToFirstPos[i].MovedPiece == p)
                                {
                                    found = movesToFirstPos[i];
                                    countToFirstPosWithSpecifiedPiece++;
                                }
                            }
                            if (countToFirstPosWithSpecifiedPiece == 1)
                            {
                                move = found;
                                return true;
                            }
                        }
                    }
                    else if (board.OwnerOf(firstPos) == board.ToMove)
                    {
                        if (Enum.TryParse<CPos>(line[2..], out var to))
                        {
                            move = new PlayerMove(board, (board.ToMove, firstPos), (board.ToMove, to));
                            return true;
                        }
                    }
                }
            }
            move = default;
            return false;
        }
    }
}
