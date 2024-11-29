using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Core.Models
{
    [Serializable]
    public class HandModel
    {
        [JsonProperty("Tiles")]
        private TileModel[] _tiles;
        
        [JsonIgnore]
        public int Size => _tiles.Length;
        
        [JsonIgnore]
        public IReadOnlyList<TileModel> Tiles => _tiles;
        
        public HandModel(int size) => _tiles = Enumerable.Repeat<TileModel>(null, size).ToArray();
        public void SetTile(int index, TileModel tileType) => _tiles[index] = tileType;
        public void RemoveTile(int index) => _tiles[index] = null;
        public void IncreaseSize() => _tiles = Enumerable.Repeat<TileModel>(null, _tiles.Length + 1).ToArray(); 
    }
}