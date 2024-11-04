using System.Collections.Generic;
using System.Linq;

namespace Core.Models
{
    public class HandModel
    {
        private readonly TileModel[] _tiles;
        public int Size { get; }
        public IReadOnlyList<TileModel> Tiles => _tiles;
        
        public HandModel(int size)
        {
            Size = size;
            _tiles = Enumerable.Repeat<TileModel>(null, size).ToArray(); 
        }
        
        public void SetTile(int index, TileModel tileType) => _tiles[index] = tileType;
        public void RemoveTile(int index) => _tiles[index] = null;
    }
}