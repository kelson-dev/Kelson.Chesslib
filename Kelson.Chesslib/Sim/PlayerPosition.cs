using System;

namespace Kelson.Chesslib.Sim
{
    /// <summary>
    /// Defines a players position on a chess board with the standard rank and file orientation
    /// </summary>
    public readonly struct PlayerPosition : IEquatable<PlayerPosition>
    {
        private readonly byte _rank;
        public int Rank => _rank;
        private readonly byte _file;
        public int File => _file;
        private readonly Player _player;
        public Player Player => _player;

        public int Index => (_rank << 3) + _file;

        public PlayerPosition(Player player, CPos pos)
        {
            var (rank, file) = pos.Coordinates();
            _rank = (byte)rank;
            _file = (byte)file;
            _player = player;
        }

        public PlayerPosition(Player player, int rank, int file)
        {
            _rank = (byte)rank;
            _file = (byte)file;
            _player = player;
        }

        public CPos ToCPos() => (CPos)Index;

        public PlayerPosition OfOpponent() => new(_player.Other(), _rank, _file);

        public NormalizedPlayerPosition Normalize() => new(this);
        public bool IsPositionallyEquivilentTo(in PlayerPosition other) => Index == other.Index;
        public bool IsPositionallyEquivilentTo(in NormalizedPlayerPosition other) => Normalize().IsPositionallyEquivilentTo(in other);

        public static implicit operator PlayerPosition((Player player, CPos pos) tuple) => new (tuple.player, tuple.pos);

        public override string ToString() => ToCPos().ToString().ToLowerInvariant();

        public static bool operator ==(PlayerPosition a, PlayerPosition b) => a.Equals(b);
        public static bool operator !=(PlayerPosition a, PlayerPosition b) => !a.Equals(b);
        
        public bool Equals(PlayerPosition other) => _rank == other._rank && _file == other._file && _player == other.Player;
        public override bool Equals(object? obj) => obj is PlayerPosition position && Equals(position);

        public override int GetHashCode() => (_rank, _file, _player).GetHashCode();
    }

    /// <summary>
    /// For Player 1, identical to StandardPosition
    /// For Player 2, vertically flipped
    /// Enables special moves such as castling and en-passant to be defined once from the perspective of Player1
    /// </summary>
    public readonly struct NormalizedPlayerPosition
    {
        private readonly byte _rank;
        public int Rank => _rank;
        private readonly byte _file;
        public int File => _file;
        private readonly Player _player;
        public Player Player => _player;

        private int absoluteRank => _player == Player.One ? _rank : 7 - _rank;

        public int Index => (absoluteRank << 3) + _file;

        public CPos ToCPos() => (CPos)((_rank << 3) + _file);
        public CPos ToAbsoluteCPos() => (CPos)Index;

        public NormalizedPlayerPosition OfOpponent() => new(_player.Other(), absoluteRank, _file);

        public NormalizedPlayerPosition(Player player, CPos pos)
        {
            var (rank, file) = pos.Coordinates();
            _rank = player == Player.One ? (byte)rank : (byte)(7 - rank);
            _file = (byte)file;
            _player = player;
        }

        public NormalizedPlayerPosition(PlayerPosition denormalized) : this(denormalized.Player, denormalized.Rank, denormalized.File)
        {
        }

        public NormalizedPlayerPosition(Player player, int absoluteRank, int file)
        {
            _rank = player == Player.One ? (byte)absoluteRank : (byte)(7 - absoluteRank);
            _file = (byte)file;
            _player = player;
        }

        public static NormalizedPlayerPosition operator +(NormalizedPlayerPosition p, (int dr, int df) vector) =>
            p.Player == Player.One
            ? new(p.Player, p.absoluteRank + vector.dr, p.File + vector.df)
            : new(p.Player, p.absoluteRank - vector.dr, p.File + vector.df);

        public PlayerPosition Denormalize() => new(_player, absoluteRank, _file);

        public bool IsPositionallyEquivilentTo(in PlayerPosition other) => IsPositionallyEquivilentTo(other.Normalize());

        public bool IsPositionallyEquivilentTo(in NormalizedPlayerPosition other) => Index == other.Index;
    }
}
