using Cysharp.Threading.Tasks;
using Modules.CheatService;
using Modules.CheatService.Controls;
using Modules.PlatformService;
using Modules.ServiceLocator;

namespace Cheats
{
    public class AdCheatsProvider : ICheatsProvider
    {
        private readonly CheatButton _showRewardedInterstitial;
        private readonly CheatButton _showInterstitial;
        private readonly CheatButton _showRewardedVideo;
        
        private readonly CheatLabel _isRewardedAdShownLabel;
        private readonly CheatLabel _isInterstitialAdShownLabel;
        private readonly CheatLabel _isRewardedVideoAdShownLabel;
        
        private bool _isRewardedAdShown;
        private bool _isInterstitialAdShown;
        private bool _isRewardedVideoAdShown;

        public AdCheatsProvider(ICheatService cheatService)
        {
            _showRewardedInterstitial = new CheatButton( "Show Rewarded Interstitial", () =>
            {
                Container.Resolve<IPlatformService>()
                    .ShowRewardedInterstitial(Bootstrapper.SessionToken)
                    .ContinueWith(x => _isRewardedAdShown = x);
            });
            _isRewardedAdShownLabel = new CheatLabel(()=> $"Is Rewarded Interstitial Shown: {_isRewardedAdShown}");
            
            _showInterstitial = new CheatButton( "Show Interstitial", () =>
            {
                Container.Resolve<IPlatformService>()
                    .ShowInterstitial(Bootstrapper.SessionToken)
                    .ContinueWith(x => _isInterstitialAdShown = x);
            });
            _isInterstitialAdShownLabel = new CheatLabel(()=> $"Is Interstitial Shown: {_isInterstitialAdShown}");
            
            _showRewardedVideo = new CheatButton( "Show Rewarded Video", () =>
            {
                Container.Resolve<IPlatformService>()
                    .ShowRewardedVideo(Bootstrapper.SessionToken)
                    .ContinueWith(x => _isRewardedVideoAdShown = x);
            });
            _isRewardedVideoAdShownLabel = new CheatLabel(()=> $"Is Rewarded Video Shown: {_isRewardedVideoAdShown}");
        }

        public void OnGUI()
        {
            _showInterstitial.OnGUI();
            _isInterstitialAdShownLabel.OnGUI();
            
            _showRewardedInterstitial.OnGUI();
            _isRewardedAdShownLabel.OnGUI();
            
            _showRewardedVideo.OnGUI();
            _isRewardedVideoAdShownLabel.OnGUI();
        }

        public string Id => "Advertising";
    }
}