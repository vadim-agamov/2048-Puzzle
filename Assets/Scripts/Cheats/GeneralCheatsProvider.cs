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
            
            _installDate = new CheatLabel(() => $"Install date: {_playerDataService.PlayerData?.InstallDate}");
            _lastSessionDate = new CheatLabel(() => $"Last Session date: {_playerDataService.PlayerData?.LastSessionDate}");
            _adsLastShownDate = new CheatLabel(() => $"Ads Shown date: {_playerDataService.PlayerData?.AdsLastShownDate}");
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