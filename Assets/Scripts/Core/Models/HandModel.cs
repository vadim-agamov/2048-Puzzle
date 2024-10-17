using System.Linq;

namespace Core.Models
{
    public class HandModel
    {
        public int Size { get; }
        public TileModel[] Tiles { get; }
        
        public HandModel(int size)
        {
            Size = size;
            Tiles = Enumerable.Repeat(new TileModel(TileType.None), size).ToArray(); 
        }
        
        public void SetTile(int index, TileModel model) => Tiles[index] = model;
    }
}