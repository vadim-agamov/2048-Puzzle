using Modules.Extensions;
using UnityEngine;

namespace Core.Models
{
    public class BoardModel : ComponentModel
    {
        public Vector2Int Size { get; }
        
        public TileModel[,] Tiles { get; }
        
        public HandModel Hand { get; }
        
        public BoardModel(Vector2Int size)
        {
            Size = size;
            Tiles = new TileModel[size.x, size.y];
            Hand = new HandModel(3);
        }
    }
}