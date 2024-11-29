using Core.Controller;
using Cysharp.Threading.Tasks;
using Modules.DiContainer;
using Modules.SoundService;
using Modules.UIService;
using Services.GamePlayerDataService;

namespace UI
{
    public class CoreHUDModel : UIModel
    {
        public int Score => Controller.Score;
        public int TopScore => Data.PlayerData.MaxScore;
        
        public bool Sound => Data.PlayerData.SoundEnabled;
        public void SetSoundEnabled(bool enabled)
        {
            Data.PlayerData.SoundEnabled = !enabled;
            Data.Commit();
        }

        public bool Music => Data.PlayerData.MusicEnabled;
        public void SetMusicEnabled(bool enabled)
        {
            var soundId = "ambient";
            Data.PlayerData.MusicEnabled = !enabled;
            Data.Commit();
            if (Data.PlayerData.MusicEnabled)
            {
                SoundService.PlayLoop(soundId);
            }
            else
            {
                SoundService.Stop(soundId);
            }
        }

        private BoardController Controller { get; set; }
        private GamePlayerDataService Data { get; set; }
        private ISoundService SoundService { get; set; }

        [Inject]
        private void Inject(
            BoardController controller, 
            GamePlayerDataService playerDataService,
            ISoundService soundService)
        {
            Controller = controller;
            Data = playerDataService;
            SoundService = soundService;
        }

        public CoreHUDModel() => Container.Inject(this);

        public void RestartGame() => Controller.Restart().Forget();
    }
}