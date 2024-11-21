using System.Threading;
using Core.Controller;
using Core.Views;
using Cysharp.Threading.Tasks;
using Modules.CheatService;
using Modules.DiContainer;
using Modules.Fsm;
using Modules.Initializator;
using Modules.SoundService;
using Modules.UIService;
using Services.GamePlayerDataService;
using Services.JumpScreenService;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.State
{
    public class CoreState : IState
    {
        private IUIService UiService { get; set; }
        private ICheatService CheatService { get; set; }
        private ISoundService SoundService { get; set; }
        private IJumpScreenService JumpScreenService { get; set; }
        private GamePlayerDataService GamePlayerDataService { get; set; }
        private CoreHUDModel _coreHUDModel;
        private BoardController _controller;
        
        [Inject]
        private void Inject(
            IUIService uiService, 
            ICheatService cheatService, 
            ISoundService soundService, 
            IJumpScreenService jumpScreenService,
            GamePlayerDataService gamePlayerDataService)
        {
            UiService = uiService;
            CheatService = cheatService;
            SoundService = soundService;
            JumpScreenService = jumpScreenService;
            GamePlayerDataService = gamePlayerDataService;
        }

        public CoreState()
        {
            Container.Inject(this);
        }

        async UniTask IState.Enter(CancellationToken cancellationToken)
        {
            await SceneManager.LoadSceneAsync("Scenes/Core");
            UiService.Canvas.sortingLayerName = "GameplayForeground";
            
            _controller = Container.BindAndInject(new BoardController(Object.FindFirstObjectByType<BoardView>()));
            _coreHUDModel = new CoreHUDModel();
            await _coreHUDModel.OpenAndShow("CoreHUD", cancellationToken);
            
            await new Initializator(_controller).Do(cancellationToken);
            
            if (GamePlayerDataService.PlayerData.MusicEnabled)
            {
                SoundService.PlayLoop("ambient");
            }
#if DEV
            CheatService.RegisterCheatProvider(_controller);
#endif 
        }

        async UniTask IState.Exit(CancellationToken cancellationToken)
        {
            await _coreHUDModel.HideAndClose(cancellationToken);
            Container.UnBind<BoardController>();

            if (GamePlayerDataService.PlayerData.MusicEnabled)
            {
                SoundService.Stop("ambient");
            }
#if DEV
            CheatService.UnRegisterCheatProvider(_controller);
#endif
        }
    }
}