using Modules.Extensions;
using UnityEngine;

namespace Core.Models
{
    public class BoardModel
    {
        public Vector2Int Size { get; }
        
        public TileType[,] Tiles { get; }
        
        public HandModel Hand { get; }
        
        public BoardModel(Vector2Int size)
        {
            Size = size;
            Tiles = new TileType[size.x, size.y].Fill(TileType.None);
            Hand = new HandModel(4);
        }
    }
}