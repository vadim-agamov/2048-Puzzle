using System.Threading;
using Core.Controller;
using Core.Views;
using Cysharp.Threading.Tasks;
using Modules.CheatService;
using Modules.FSM;
using Modules.Initializator;
using Modules.ServiceLocator;
using Modules.SoundService;
using Modules.UIService;
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
        private CoreHUDModel _coreHUDModel;
        private BoardController _controller;
        
        [Inject]
        private void Inject(IUIService uiService, ICheatService cheatService, ISoundService soundService)
        {
            UiService = uiService;
            CheatService = cheatService;
            SoundService = soundService;
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
            SoundService.PlayLoop("ambient");

            await new Initializator(_controller).Do(cancellationToken);
#if DEV
            CheatService.RegisterCheatProvider(_controller);
#endif 
        }

        async UniTask IState.Exit(CancellationToken cancellationToken)
        {
            await _coreHUDModel.HideAndClose(cancellationToken);
            Container.UnBind<BoardController>();
            SoundService.Stop("ambient");
#if DEV
            CheatService.UnRegisterCheatProvider(_controller);
#endif
        }
    }
}