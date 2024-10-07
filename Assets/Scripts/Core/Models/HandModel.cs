using System.Linq;

namespace Core.Models
{
    public class HandModel
    {
        public int Size { get; }
        public TileType[] Tiles { get; }
        
        public HandModel(int size)
        {
            Size = size;
            Tiles = Enumerable.Repeat(TileType.None, size).ToArray(); 
        }
        
        public void SetTile(int index, TileType tileType) => Tiles[index] = tileType;
    }
}