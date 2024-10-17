using System.Threading;
using Core.Controller;
using Core.Views;
using Cysharp.Threading.Tasks;
using Modules.FSM;
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
            await ServiceLocator.Register(new BoardController(boardView)).Initialize(cancellationToken);
        }

        async UniTask IState.Exit(CancellationToken cancellationToken)
        {
            ServiceLocator.UnRegister<BoardController>();
        }
    }
}