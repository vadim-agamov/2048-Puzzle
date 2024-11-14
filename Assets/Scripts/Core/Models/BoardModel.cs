using System.Linq;
using Modules.Extensions;
using UnityEngine;

namespace Core.Models
{
    public class BoardModel
    {
        public Vector2Int Size { get; }
        
        public TileModel[,] Tiles { get; }
        
        public HandModel Hand { get; }
        
        public int Score => Tiles.Where(m => m != null).Sum(m => m.Type.Score());
        
        public BoardModel(Vector2Int size)
        {
            Size = size;
            Tiles = new TileModel[size.x, size.y];
            Hand = new HandModel(2);
        }
    }
}