using UnityEngine;

namespace Core.Models
{
    public class BoardModel
    {
        public Vector2Int Size => new Vector2Int(Tiles.GetLength(0), Tiles.GetLength(1));
        
        public TileModel[,] Tiles { get; }
        
        public HandModel Hand { get; }
        
        public int Score { get; private set; }
        
        public void AddScore(int value)
        {
            Debug.Assert(value >= 0);
            Score += value;
        }
        
        public BoardModel(Vector2Int size)
        {
            Tiles = new TileModel[size.x, size.y];
            Hand = new HandModel(1);
        }
    }
}