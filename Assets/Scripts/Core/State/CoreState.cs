using System.Threading;
using Core.Controller;
using Core.Views;
using Cysharp.Threading.Tasks;
using Modules.FSM;
using Modules.Initializator;
using Modules.ServiceLocator;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.State
{
    public class CoreState : IState
    {
        async UniTask IState.Enter(CancellationToken cancellationToken)
        {
            await SceneManager.LoadSceneAsync("Scenes/Core");
            var boardView = Object.FindObjectOfType<BoardView>();
            var controller = ServiceLocator.Bind(new BoardController(boardView));
            
            await new Initializator(controller).Do(cancellationToken);
        }

        async UniTask IState.Exit(CancellationToken cancellationToken)
        {
            ServiceLocator.UnBind<BoardController>();
        }
    }
}