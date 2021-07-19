using Kelson.Chesslib.Sim;
using Kelson.Chesslib.Sim.Rules;
using Kelson.Chesslib.Tests.Utilities;
using static Kelson.Chesslib.Sim.Piece;
using static Kelson.Chesslib.CPos;

namespace Kelson.Chesslib.Tests.Pieces
{
    public class PawnTests
    {
        [Theory]
        [InlineData(Player.One, E3, E4)]
        [InlineData(Player.One, A2, A3)]
        [InlineData(Player.Two, E3, E2)]
        [InlineData(Player.Two, E7, E6)]
        public void PawnSingleAdvanceShouldHaveOneResult(Player player, CPos start, CPos expected)
        {
            Span<PlayerMove> buffer = stackalloc PlayerMove[24];
            var rule = new PawnSingleAdvanceRule();
            var board = Board.With(player, Pawn).At(start);
            
            rule.EnumerateMoves(board, (player, start), ref buffer);
            buffer.Length.Should().Be(1);
            buffer[0].To.ToCPos().Should().Be(expected);
        }

        [Theory]
        [InlineData(Player.One, E7, E8)]
        [InlineData(Player.Two, E2, E1)]
        public void PawnSingleAdvanceToPromotionHas4ResultsToSameDestination(Player player, CPos start, CPos expected)
        {
            Span<PlayerMove> buffer = stackalloc PlayerMove[24];
            var rule = new PawnSingleAdvanceRule();
            var board = Board.With(player, Pawn).At(start);

            rule.EnumerateMoves(board, (player, start), ref buffer);
            buffer.Length.Should().Be(4);
            for (int i = 0; i < buffer.Length; i++)
                buffer[i].To.ToCPos().Should().Be(expected);
        }

        [Theory]
        [InlineData(Player.One, E2, E4)]
        [InlineData(Player.One, A2, A4)]
        [InlineData(Player.Two, E7, E5)]
        [InlineData(Player.Two, A7, A5)]
        public void PawnDoubleAdvanceShouldHaveOneResult(Player player, CPos start, CPos expected)
        {
            Span<PlayerMove> buffer = stackalloc PlayerMove[24];
            var rule = new PawnDoubleAdvanceRule();
            var board = Board.With(player, Pawn).At(start);
            
            rule.EnumerateMoves(board, (player, start), ref buffer);
            buffer.Length.Should().Be(1);
            buffer[0].To.ToCPos().Should().Be(expected);
        }

        [Theory]
        [InlineData(Player.One, E2, 1)]
        [InlineData(Player.One, A2, 1)]
        [InlineData(Player.One, E3, 0)]
        [InlineData(Player.One, A3, 0)]
        [InlineData(Player.One, A7, 0)]
        [InlineData(Player.One, E7, 0)]
        [InlineData(Player.Two, E2, 0)]
        [InlineData(Player.Two, A2, 0)]
        [InlineData(Player.Two, E3, 0)]
        [InlineData(Player.Two, A3, 0)]
        [InlineData(Player.Two, A7, 1)]
        [InlineData(Player.Two, E7, 1)]
        public void PawnDoubleAdvanceShouldOnlyAllowStartingRank(Player player, CPos start, int count)
        {
            Span<PlayerMove> buffer = stackalloc PlayerMove[24];
            var rule = new PawnDoubleAdvanceRule();
            var board = Board.With(player, Pawn).At(start);
            rule.EnumerateMoves(board, (player, start), ref buffer);
            buffer.Length.Should().Be(count);
        }
    }
}