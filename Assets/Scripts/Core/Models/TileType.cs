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
    }
}