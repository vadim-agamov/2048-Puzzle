using System.Linq;
using Core.Models;
using Core.Views;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = "Create TilesConfig", fileName = "TilesConfig", order = 0)]
    public class TilesConfig : ScriptableObject
    {
        [SerializeField]
        private TileView[] _tiles;
        
        public TileView GetPrefab(TileType type) => _tiles.Single(x => x.Type == type);
    }
}