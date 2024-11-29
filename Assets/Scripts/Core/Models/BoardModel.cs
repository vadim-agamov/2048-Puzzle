using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Models
{
    [Serializable]
    public class BoardModel
    {
        [JsonProperty("Tiles")] 
        private TileModel[,] _tiles;
        
        [JsonProperty("Score")]
        private int _score;
        
        [JsonProperty("Hand")]
        private HandModel _hand;
        
        [JsonIgnore]
        public TileModel[,] Tiles => _tiles;

        [JsonIgnore]
        public HandModel Hand => _hand;

        [JsonIgnore] 
        public int Score => _score;

        [JsonIgnore]
        public Vector2Int Size => new Vector2Int(Tiles.GetLength(0), Tiles.GetLength(1));

        public void AddScore(int value)
        {
            Debug.Assert(value >= 0);
            _score += value;
        }
        
        public BoardModel()
        {
        }
        
        public BoardModel(Vector2Int size)
        {
            _tiles = new TileModel[size.x, size.y];
            _hand = new HandModel(1);
        }
    }
}