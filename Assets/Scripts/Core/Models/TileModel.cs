using Modules.Extensions;

namespace Core.Models
{
    public class TileModel : ComponentModel
    {
        public TileType Type { get; }

        public TileModel(TileType type)
        {
            Type = type;
        }
    }
}