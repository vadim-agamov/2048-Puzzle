using System;
using Modules.ComponentWithModel;
using Modules.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Models
{
    [Serializable]
    public class TileModel : ComponentModel
    {
        [JsonProperty(nameof(HandPosition))]
        private int _handPosition;
                
        [JsonProperty(nameof(BoardPosition)),  JsonConverter(typeof(Vector2IntConverter))]
        private Vector2Int _boardPosition;
        
        [JsonProperty(nameof(Type))]
        private TileType _type;

        [JsonIgnore]
        public TileType Type => _type;

        [JsonIgnore]
        public Vector2Int BoardPosition => _boardPosition;
       
        [JsonIgnore]
        public int HandPosition => _handPosition;
        
        public TileModel()
        {
        }

        public TileModel(TileType type, Vector2Int boardPosition)
        {
            _type = type;
            _boardPosition = boardPosition;
        }
        
        public TileModel(TileType type, int hadPosition)
        {
            _type = type;
            _handPosition = hadPosition;
        }
        
        public TileModel WithPosition(Vector2Int boardPosition) => new (Type, boardPosition);
        public TileModel WithType(TileType type) => new (type, BoardPosition);
    }
}