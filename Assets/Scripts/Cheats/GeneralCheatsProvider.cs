using Modules.CheatService;
using Modules.CheatService.Controls;
using Modules.PlayerDataService;
using Services.GamePlayerDataService;
using UnityEngine;

namespace Cheats
{
    public class GeneralCheatsProvider : ICheatsProvider
    {
        private string _id;
        private readonly GamePlayerDataService _playerDataService;
        private readonly CheatButton _reset;
        private readonly CheatLabel _installDate;
        private readonly CheatLabel _lastSessionDate;
        private readonly CheatLabel _adsLastShownDate;

        public GeneralCheatsProvider(ICheatService cheatService, GamePlayerDataService playerDataService)
        {
            _playerDataService = playerDataService;

            _reset = new CheatButton( "Reset", () =>
            {
                Debug.Log("Reset");
                _playerDataService.ResetData();
            });
            
            _installDate = new CheatLabel(() => $"Install: {_playerDataService.PlayerData?.InstallDate:d.M.yy}");
            _lastSessionDate = new CheatLabel(() => $"Session: {_playerDataService.PlayerData?.LastSessionDate:d.M.yy}");
            _adsLastShownDate = new CheatLabel(() => $"Ads: {_playerDataService.PlayerData?.AdsLastShownDate:d.M.yy}");
        }

        void ICheatsProvider.OnGUI()
        {
            _installDate.OnGUI();
            _lastSessionDate.OnGUI();
            _adsLastShownDate.OnGUI();
            _reset.OnGUI();
        }

        string ICheatsProvider.Id => "General";
    }
}