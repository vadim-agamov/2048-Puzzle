using Modules.Extensions;
using UnityEngine;

namespace Core.Models
{
    public class TileModel : ComponentModel
    {
        public TileType Type { get; }
        public Vector2Int BoardPosition { get; }
        public int HandPosition { get;}

        public TileModel(TileType type, Vector2Int boardPosition)
        {
            Type = type;
            BoardPosition = boardPosition;
        }
        
        public TileModel(TileType type, int hadPosition)
        {
            Type = type;
            HandPosition = hadPosition;
        }
        
        public TileModel WithPosition(Vector2Int boardPosition) => new (Type, boardPosition);
        public TileModel WithType(TileType type) => new (Type, BoardPosition);
    }
}