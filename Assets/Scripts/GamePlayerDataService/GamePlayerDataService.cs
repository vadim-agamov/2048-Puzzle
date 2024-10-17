using Modules.PlayerDataService;

namespace Services.GamePlayerDataService
{
    public class GamePlayerDataService: PlayerDataService<PlayerData>
    {
        public IPropertyProvider<bool> MuteSoundProperty => new PropertyProvider<bool>(
            () => Data.MuteSound,
            val =>
            {
                Data.MuteSound = val;
                SetDirty();
            });

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