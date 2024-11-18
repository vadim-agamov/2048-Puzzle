using Modules.CheatService;
using Modules.CheatService.Controls;
using Modules.LocalizationService;
using Modules.PlatformService;
using Modules.ServiceLocator;

namespace Cheats
{
    public class LocalizationCheatsProvider: ICheatsProvider
    {
        private readonly CheatButton _langRussian;
        private readonly CheatButton _langEnglish;
        private readonly CheatButton _langTurkish;
        private readonly CheatLabel _currentLang;
        private ILocalizationService LocalizationService { get; }
        
        public LocalizationCheatsProvider(ICheatService cheatService, ILocalizationService localizationService)
        {
            LocalizationService = localizationService;
            
            _langRussian = new CheatButton( Language.Russian.ToString(),
                () => LocalizationService.SetLanguage(Language.Russian),
                false);
            _langEnglish = new CheatButton( Language.English.ToString(),
                () => LocalizationService.SetLanguage(Language.English),
                false);
            _langTurkish = new CheatButton( Language.Turkish.ToString(),
                () => LocalizationService.SetLanguage(Language.Turkish),
                false);

            _currentLang = new CheatLabel(() => $"Current language: {LocalizationService.CurrentLanguage}");
        }

        public void OnGUI()
        {
            _langRussian.OnGUI();
            _langEnglish.OnGUI();
            _langTurkish.OnGUI();
            _currentLang.OnGUI();
        }

        public string Id => "Localization";
    }
}