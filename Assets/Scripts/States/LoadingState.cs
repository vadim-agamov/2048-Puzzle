using System;
using System.Collections.Generic;
using System.Threading;
using Cheats;
using Core.State;
using Cysharp.Threading.Tasks;
using Modules.AnalyticsService;
using Modules.CheatService;
using Modules.FlyItemsService;
using Modules.Fsm;
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

            IJumpScreenService jumpScreenService = Container.BindAndInject(GameObject.Find("JumpScreen").GetComponent<JumpScreen>());
            await jumpScreenService.Show(token);
            
#if UNITY_EDITOR
            var platformService = Container.BindAndInject<IPlatformService>(new GameObject("EditorSN").AddComponent<EditorPlatformService>());
#elif FB
            IPlatformService platformService = new GameObject("FbBridge").AddComponent<FbPlatformService>();
#elif YANDEX
            var platformService = Container.BindAndInject<IPlatformService>(new GameObject("Yandex").AddComponent<YandexPlatformService>());
#elif CRAZY
            IPlatformService platformService = new CrazyPlatformService();
#elif DUMMY_WEBGL
            Container.BindAndInject<IPlatformService>(new GameObject("DummySN").AddComponent<DummyPlatformService>());
#endif

            Container.BindAndInject<IUIService>(new UIService(new Vector2(1000, 1500)));
            var playerDataService = Container.BindAndInject(new GameObject().AddComponent<GamePlayerDataService>());
            var localizationService = Container.BindAndInject<ILocalizationService>(new LocalizationService());
            var analyticsService = Container.BindAndInject<IAnalyticsService>(new AnalyticsService());
            Container.BindAndInject<IFlyItemsService>(new FlyItemsService());
            Container.BindAndInject<ISoundService>(new GameObject().AddComponent<SoundService>());
            
#if DEV
            RegisterCheats(localizationService, playerDataService);
#endif
            
                        
            var initializableServices = Container.AllServices<IInitializable>();
            await new Initializator(initializableServices).Do(token, jumpScreenService);
            
            Debug.Log($"[{nameof(LoadingState)}] all services registered");

            var newInstall = playerDataService.PlayerData.InstallDate == playerDataService.PlayerData.LastSessionDate;
            playerDataService.PlayerData.LastSessionDate = DateTime.Now;
            playerDataService.Commit();
            
            analyticsService.Start();
            
            await Fsm.Enter(new CoreState(), token);
            
            await jumpScreenService.Hide(token);
            
            platformService.GameReady();
            analyticsService.TrackEvent("loaded", new Dictionary<string, object>
            {
                { "new_install", newInstall }
            });
        }
        
        private static void SetupEventSystem()
        {
            var eventSystemPrefab = Resources.Load("EventSystem");
            var eventSystem = GameObject.Instantiate(eventSystemPrefab);
            eventSystem.name = "[EventSystem]";
            GameObject.DontDestroyOnLoad(eventSystem);
        }

#if DEV
        private void RegisterCheats(ILocalizationService localizationService, GamePlayerDataService playerDataService)
        {
            ICheatService cheatService = new GameObject().AddComponent<CheatService>();
            Container.BindAndInject(cheatService);
            cheatService.RegisterCheatProvider(new GeneralCheatsProvider(cheatService, playerDataService));
            cheatService.RegisterCheatProvider(new AdCheatsProvider(cheatService));
            // cheatService.RegisterCheatProvider(new LocalizationCheatsProvider(cheatService, localizationService));
        }  
#endif
        
        UniTask IState.Exit(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}