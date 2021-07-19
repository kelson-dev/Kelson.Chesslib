using Kelson.Chesslib.Sim;
using static Kelson.Chesslib.CPos;

namespace Kelson.Chesslib.Tests
{
    public class NormalizationTests
    {
        [Theory]
        [InlineData(A1, A8)]
        [InlineData(E4, E5)]
        public void Player2NormalizationShouldGetExpectedPosition(CPos start, CPos expected)
        {
            var initial = new PlayerPosition(Player.Two, start);
            var normalized = initial.Normalize();
            var (erank, efile) = expected.Coordinates();
            normalized.Rank.Should().Be(erank);
            normalized.File.Should().Be(efile);
            normalized.ToCPos().Should().Be(expected);
        }

        [Theory]
        [InlineData(Player.One, E2)]
        [InlineData(Player.Two, E2)]
        [InlineData(Player.One, A1)]
        [InlineData(Player.One, H1)]
        [InlineData(Player.Two, A1)]
        [InlineData(Player.Two, H1)]
        public void PositionNormalizationIsReversable(Player player, CPos position)
        {
            var initial = new PlayerPosition(player, position);
            var normalized = initial.Normalize();
            normalized.Index.Should().Be(initial.Index, because: "Absolute board location is invariant under normalization");
            if (player == Player.Two)
                normalized.Rank.Should().NotBe(initial.Rank, because: "Normalization constitutes a flip around the horizontal axis for player 2");
            else
                normalized.Rank.Should().Be(initial.Rank, because: "Standard positions are already normalized to player ones perspective");
            normalized.File.Should().Be(initial.File, because: "Normalization is a flip, not a rotation. File is invariant under normalization");
            var denorm = normalized.Denormalize();
            denorm.Index.Should().Be(initial.Index);
        }
    }
}
