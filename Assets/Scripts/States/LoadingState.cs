using System.Threading;
using Cheats;
using Core.State;
using Cysharp.Threading.Tasks;
using Modules.AnalyticsService;
using Modules.CheatService;
using Modules.FlyItemsService;
using Modules.FSM;
using Modules.Initializator;
using Modules.LocalizationService;
using Modules.PlatformService;
using Modules.ServiceLocator;
using Modules.SoundService;
using Modules.UIService;
using Services.GamePlayerDataService;
using Services.JumpScreenService;
using UnityEngine;

#if UNITY_EDITOR
    using Modules.PlatformService.EditorPlatformService;
#elif FB
    using Modules.PlatformService.FbPlatformService;
#elif YANDEX
    using Modules.PlatformService.YandexPlatformService;
#elif CRAZY
    using Modules.PlatformService.CrazyGamesPlatformService;
#elif DUMMY_WEBGL
    using Modules.PlatformService.DummyPlatformService;
#endif

namespace States
{
    public class LoadingState : IState
    {
        async UniTask IState.Enter(CancellationToken token)
        {
            SetupEventSystem();

            IJumpScreenService jumpScreenService = ServiceLocator.Bind(GameObject.Find("JumpScreen").GetComponent<JumpScreen>());
            await jumpScreenService.Show(token);
            
#if UNITY_EDITOR
            ServiceLocator.Bind<IPlatformService>(new GameObject("EditorSN").AddComponent<EditorPlatformService>());
#elif FB
            IPlatformService platformService = new GameObject("FbBridge").AddComponent<FbPlatformService>();
#elif YANDEX
            IPlatformService platformService = new GameObject("Yandex").AddComponent<YandexPlatformService>();
#elif CRAZY
            IPlatformService platformService = new CrazyPlatformService();
#elif DUMMY_WEBGL
            IPlatformService platformService = new GameObject("DummySN").AddComponent<DummyPlatformService>();
#endif

            ServiceLocator.Bind<IUIService>(new UIService(new Vector2(1080, 1920)));
            ServiceLocator.Bind(new GameObject().AddComponent<GamePlayerDataService>());
            var localizationService = ServiceLocator.Bind<ILocalizationService>(new LocalizationService());
            ServiceLocator.Bind<IAnalyticsService>(new AnalyticsService());
            ServiceLocator.Bind<IFlyItemsService>(new FlyItemsService());
            ServiceLocator.Bind<ISoundService>(new GameObject().AddComponent<SoundService>());
            
#if DEV
            RegisterCheats(localizationService);
#endif
            
                        
            var initializableServices = ServiceLocator.AllServices<IInitializable>();
            await new Initializator(initializableServices).Do(token, jumpScreenService);
            
            Debug.Log($"[{nameof(LoadingState)}] all services registered");

            // var newInstall = playerDataService.PlayerData.InstallDate == playerDataService.PlayerData.LastSessionDate;
            // playerDataService.PlayerData.LastSessionDate = DateTime.Now;
            // playerDataService.Commit();
            
            // ServiceLocator.Get<IAnalyticsService>().Start();
            
            await Fsm.Enter(new CoreState(), token);
            
            await jumpScreenService.Hide(token);
            
            // platformService.GameReady();
            // ServiceLocator.Get<IAnalyticsService>().TrackEvent($"Loaded", new Dictionary<string, object>
            // {
                // { "new_install", newInstall }
            // });
        }
        
        private static void SetupEventSystem()
        {
            var eventSystemPrefab = Resources.Load("EventSystem");
            var eventSystem = GameObject.Instantiate(eventSystemPrefab);
            eventSystem.name = "[EventSystem]";
            GameObject.DontDestroyOnLoad(eventSystem);
        }

#if DEV
        private void RegisterCheats(ILocalizationService localizationService)
        {
            ICheatService cheatService = new GameObject().AddComponent<CheatService>();
            ServiceLocator.Bind(cheatService);
            // cheatService.RegisterCheatProvider(new GeneralCheatsProvider(cheatService, ServiceLocator.Get<GamePlayerDataService>()));
            cheatService.RegisterCheatProvider(new AdCheatsProvider(cheatService));
            cheatService.RegisterCheatProvider(new LocalizationCheatsProvider(cheatService, localizationService));
        }  
#endif
        
        UniTask IState.Exit(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}