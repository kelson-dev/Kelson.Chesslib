using Kelson.Common.DataStructures.Sets;

namespace Kelson.Chesslib
{
    public enum Player : byte
    {
        One = 0,
        Two = 1,
    }

    public static class PlayerExtensions
    {
        public static Player Other(this Player player) => (Player)(~(int)player & 1);
    }
}
