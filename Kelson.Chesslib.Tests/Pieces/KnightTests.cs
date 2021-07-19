using static Kelson.Chesslib.Sim.Piece;
using static Kelson.Chesslib.CPos;
using Kelson.Chesslib.Sim;
using Kelson.Chesslib.Sim.Rules;
using Kelson.Chesslib.Tests.Utilities;

namespace Kelson.Chesslib.Tests.Pieces
{
    public class KnightTests
    {
        [Theory]
        [InlineData(A1)]
        [InlineData(A8)]
        [InlineData(H1)]
        [InlineData(H8)]
        public void TestKnightsInCornersHaveExactlyTwoMoves(CPos position)
        {
            Span<PlayerMove> buffer = stackalloc PlayerMove[24];
            var rule = new KnightMoves();
            var board = Board.With(Player.One, Knight).At(position);

            rule.EnumerateMoves(board, (Player.One, position), ref buffer);
            buffer.Length.Should().Be(2);
        }

        [Theory]
        [InlineData(A3)]
        [InlineData(A4)]
        [InlineData(D1)]
        [InlineData(E1)]
        [InlineData(H3)]
        [InlineData(H4)]
        [InlineData(D8)]
        [InlineData(E8)]
        public void TestKnightsOnEdgesHaveExactly4Moves(CPos position)
        {
            Span<PlayerMove> buffer = stackalloc PlayerMove[24];
            var rule = new KnightMoves();
            var board = Board.With(Player.One, Knight).At(position);

            rule.EnumerateMoves(board, (Player.One, position), ref buffer);
            buffer.Length.Should().Be(4);
        }

        [Theory]
        [InlineData(D4)]
        [InlineData(E4)]
        [InlineData(D5)]
        [InlineData(E5)]
        public void TestKnightsInCenterHaveExactly8Moves(CPos position)
        {
            Span<PlayerMove> buffer = stackalloc PlayerMove[24];
            var rule = new KnightMoves();
            var board = Board.With(Player.One, Knight).At(position);

            rule.EnumerateMoves(board, (Player.One, position), ref buffer);
            buffer.Length.Should().Be(8);
        }
    }
}
