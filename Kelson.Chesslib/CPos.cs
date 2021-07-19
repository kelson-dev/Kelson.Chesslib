using Kelson.Common.DataStructures.Sets;

namespace Kelson.Chesslib
{
    /// <summary>
    /// Chess Position
    /// Indexes are assigned incrementing from left to right with 0 at A1 and 63 at H8
    /// This means the lower 3 bits indicate file and the upper 3 bits indicate rank
    /// </summary>
    public enum CPos
    {
        A1 = 0, A2 =  8, A3 = 16, A4 = 24, A5 = 32, A6 = 40, A7 = 48, A8 = 56,
        B1 = 1, B2 =  9, B3 = 17, B4 = 25, B5 = 33, B6 = 41, B7 = 49, B8 = 57,
        C1 = 2, C2 = 10, C3 = 18, C4 = 26, C5 = 34, C6 = 42, C7 = 50, C8 = 58,
        D1 = 3, D2 = 11, D3 = 19, D4 = 27, D5 = 35, D6 = 43, D7 = 51, D8 = 59,
        E1 = 4, E2 = 12, E3 = 20, E4 = 28, E5 = 36, E6 = 44, E7 = 52, E8 = 60,
        F1 = 5, F2 = 13, F3 = 21, F4 = 29, F5 = 37, F6 = 45, F7 = 53, F8 = 61,
        G1 = 6, G2 = 14, G3 = 22, G4 = 30, G5 = 38, G6 = 46, G7 = 54, G8 = 62,
        H1 = 7, H2 = 15, H3 = 23, H4 = 31, H5 = 39, H6 = 47, H7 = 55, H8 = 63,
    }

    public static class CPosExtensions
    {
        public static (int rank, int file) Coordinates(this CPos pos) => (((int)pos & 0b111000) >> 3, ((int)pos & 0b111));
        public static int ToIndex(this CPos pos) => (int)pos;
        public static bool IsWhiteSquare(this CPos pos)
        {
            var flags = new ImmutableSet64((ulong)pos);
            // The 0th bit of the position index marks even and odd files
            // The 3rd bit of the position index marks even and on ranks
            // If exactly one rank or file is odd, the square is colored white
            return flags.Contains(0) ^ flags.Contains(3);
        }
    }
}
