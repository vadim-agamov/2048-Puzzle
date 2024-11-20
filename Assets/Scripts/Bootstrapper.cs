using System.Threading;
using Cysharp.Threading.Tasks;
using Modules.Fsm;
using States;
using UnityEngine;

public static class Bootstrapper
{
    private static CancellationTokenSource _applicationCancellation;

    public static CancellationToken SessionToken => _applicationCancellation.Token;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeSceneStart()
    {
        Application.targetFrameRate = 60;
        //Time.fixedDeltaTime = 1f / Application.targetFrameRate;
        Application.quitting += OnApplicationQuit;
        _applicationCancellation = new CancellationTokenSource();

        Screen.orientation = ScreenOrientation.Portrait;
        
        Fsm.Enter(new LoadingState(), _applicationCancellation.Token).Forget();
    }

    private static void OnApplicationQuit()
    {
        Application.quitting -= OnApplicationQuit;
        _applicationCancellation.Cancel();
        _applicationCancellation = null;
    }
}