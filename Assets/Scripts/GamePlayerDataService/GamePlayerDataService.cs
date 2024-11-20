using Modules.PlayerDataService;

namespace Services.GamePlayerDataService
{
    public class GamePlayerDataService: PlayerDataService<PlayerData>
    {
        public PlayerData PlayerData
        {
            get => Data;
            set
            {
                Data = value;
                SetDirty();
            }
        }
    }
}