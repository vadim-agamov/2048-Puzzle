using System;

namespace Core.Models
{
    public enum TileType
    {
        None,
        Tile1,
        Tile2,
        Tile4,
        Tile8,
        Tile16,
        Tile32,
        Tile64,
        Tile128,
        Tile256,
        Tile512,
        Tile1024,
        MaxTile = Tile1024
    }
    
    public static class TileTypeExtensions
    {
        public static TileType Next(this TileType type) => (TileType)Math.Min((int)type+1, (int)TileType.MaxTile);

        public static int Score(this TileType type) => type switch
        {
            TileType.Tile1 => 1,
            TileType.Tile2 => 2,
            TileType.Tile4 => 4,
            TileType.Tile8 => 8,
            TileType.Tile16 => 16,
            TileType.Tile32 => 32,
            TileType.Tile64 => 64,
            TileType.Tile128 => 128,
            TileType.Tile256 => 256,
            TileType.Tile512 => 512,
            TileType.Tile1024 => 1024,
            _ => 0
        };
    }
}