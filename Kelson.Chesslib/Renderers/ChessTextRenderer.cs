using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Kelson.Chesslib.Sim;
namespace Kelson.Chesslib.Renderers
{
    public class ChessTextRenderer : IChessRenderer<StringBuilder>
    {
        public const string ChessGlyphset = "♛ ♜ ♞ ♝ ♙ ♚ ♕ ♖ ♘ ♗ ♙ ♔ · █ ";
        public const string AsciiGlyphset = "Q R N B P K q r n b p k · █ ";
        public const string EmojiGlyphset =
            "\uD83D\uDC78 \uD83C\uDFF0 \uD83E\uDD93 \uD83E\uDD3A \uD83E\uDD16 \uD83E\uDD34 " +
            "\uD83D\uDC7D \uD83C\uDFEF \uD83D\uDC34 \uD83E\uDDDB \uD83D\uDC7E \uD83D\uDD79\uFE0F " +
            "\u2B1C \uD83D\uDFE5 ";

        private readonly string[] _glyphSet;
        public ChessTextRenderer(string glyphSet = AsciiGlyphset) => _glyphSet = TextElements(glyphSet).ToArray();

        private IEnumerable<string> TextElements(string glphys, string skip = " ")
        {
            var enumerator = StringInfo.GetTextElementEnumerator(glphys);
            while (enumerator.MoveNext())
            {
                string current = enumerator.GetTextElement()!;
                if (current != skip)
                    yield return current;
            }
        }

        private string Character(Chessboard board, CPos position)
        {
            var owner = board.OwnerOf(position);
            if (owner == null)
                return position.IsWhiteSquare() ? glyph(12) : glyph(13);

            var piece = board[position];

            string glyph(int i) => _glyphSet[i];


            return (owner, piece) switch
            {
                (Player.One, Piece.Queen) => glyph(0),
                (Player.One, Piece.Rook) => glyph(1),
                (Player.One, Piece.Knight) => glyph(2),
                (Player.One, Piece.Bishop) => glyph(3),
                (Player.One, Piece.Pawn) => glyph(4),
                (Player.One, Piece.King) => glyph(5),
                (Player.Two, Piece.Queen) => glyph(6),
                (Player.Two, Piece.Rook) => glyph(7),
                (Player.Two, Piece.Knight) => glyph(8),
                (Player.Two, Piece.Bishop) => glyph(9),
                (Player.Two, Piece.Pawn) => glyph(10),
                (Player.Two, Piece.King) => glyph(11),
                _ => "?"
            };
        }

        public StringBuilder Render(in Chessboard board)
        {
            var builder = new StringBuilder();

            for (int rank = 7; rank >= 0; rank--)
            {
                //builder = builder.Append((char)('1' + rank)).Append(" ");
                for (int file = 0; file < 8; file++)
                {
                    int index = (rank << 3) + file;
                    var pos = (CPos)(index);

                    builder = builder.Append(Character(board, pos));
                }

                builder = builder.AppendLine();
            }

            return builder;
        }
    }
}