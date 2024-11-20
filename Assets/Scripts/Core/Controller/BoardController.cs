using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Actions;
using Core.Models;
using Core.State;
using Core.Views;
using Cysharp.Threading.Tasks;
using Modules.Actions;
using Modules.AnalyticsService;
using Modules.CheatService;
using Modules.CheatService.Controls;
using Modules.Events;
using Modules.Extensions;
using Modules.FlyItemsService;
using Modules.Fsm;
using Modules.Initializator;
using Modules.PlatformService;
using Modules.ServiceLocator;
using Modules.SoundService;
using Modules.UIService;
using Modules.UIService.Events;
using Modules.Utils;
using Services.GamePlayerDataService;
using UI;
using UnityEngine;

namespace Core.Controller
{
    public class BoardController : IInitializable, ICheatsProvider
    {
        private readonly BoardView _boardView;
        private BoardModel _boardModel;
        private bool _addIsInProgress;
        private int _slotsAdded = 0;
        private IAnalyticsService AnalyticsService { get; set; }
        private GamePlayerDataService PlayerDataService { get; set; }
        private IFlyItemsService FlyItemsService { get; set; }
        private UIService UiService { get; set; }
        private IPlatformService PlatformService { get; set; }
        private ISoundService SoundService { get; set; }
        public int Score => _boardModel?.Score ?? 0;
        public bool IsInitialized { get; private set; }


        [Inject]
        private void Inject(
            IAnalyticsService analyticsService,
            GamePlayerDataService gamePlayerDataService,
            IFlyItemsService flyItemsService,
            UIService uiService,
            IPlatformService platformService,
            ISoundService soundService)
        {
            AnalyticsService = analyticsService;
            PlayerDataService = gamePlayerDataService;
            FlyItemsService = flyItemsService;
            UiService = uiService;
            PlatformService = platformService;
            SoundService = soundService;
        }

        public BoardController(BoardView boardView) => _boardView = boardView;

        public UniTask Initialize(CancellationToken cancellationToken)
        {
            var size = PlayerDataService.PlayerData.MaxScore switch
            {
                < 10000 => new Vector2Int(4, 4),
                < 15000 => new Vector2Int(4, 5),
                < 25000 => new Vector2Int(5, 5),
                < 35000 => new Vector2Int(5, 6),
                < 45000 => new Vector2Int(6, 6),
                _ => new Vector2Int(6, 7)
            };
            
            _boardModel = new BoardModel(size);
            _boardModel.Hand.SetTile(0, new TileModel(TileType.Tile1, 0));
            _boardView.Initialize(this, _boardModel, UiService);
            
            _cheatLabel = new CheatLabel(() => $"Score: {Score}");
            _cheatEndGameButton = new CheatButton("EndGame", () => EndGame().Forget());
            
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        
        public void PutTileOnBoard(Vector2Int position, TileView tileView)
        {
            Do().Forget();

            async UniTask Do()
            {
                var success = await new PutBlockOnBoardAction(_boardModel, _boardView, position, tileView).Do();
                if (success)
                {
                    await new ParallelAction()
                        .Add(new TryRefillHandAction(_boardModel, _boardView))
                        .Add(new TryBestMergeTileAction(_boardModel, _boardView, tileView.Model, FlyItemsService))
                        .Do();
                    
                    CheckEndGame();
                }
                else
                {
                    tileView.RestoreHandPosition();
                }
            }
        }

        public async UniTask AddSlotToHand()
        {
            if (_addIsInProgress)
            {
                Debug.LogWarning("Add slot is in progress");
                return;
            }
            
            _addIsInProgress = true;
            using var mute = Mute();
            
            try
            {
                var result = await PlatformService.ShowRewardedVideo(Bootstrapper.SessionToken);
                if (result)
                {
                    _slotsAdded++;
                    _boardModel.Hand.IncreaseSize();
                    _boardView.ReloadHand();
                    await new TryRefillHandAction(_boardModel, _boardView).Do();
                }
            }
            finally
            {
                _addIsInProgress = false;
            }
        }

        private void CheckEndGame()
        {
            if (_boardModel.Tiles.Where(m => m == null).Any())
            {
                return;
            }

            EndGame().Forget();
        }

        private async UniTask EndGame()
        {
            PlayerDataService.PlayerData.MaxScore = Math.Max(Score, PlayerDataService.PlayerData.MaxScore);
            PlayerDataService.PlayerData.GamesPlayed++;
            PlayerDataService.Commit();
            AnalyticsService.TrackEvent("end_game", new Dictionary<string, object>
            {
                { "score", Score },
                { "slots_added", _slotsAdded },
                { "board_size", $"{_boardModel.Size.x}x{_boardModel.Size.y}" },
                { "games_played", PlayerDataService.PlayerData.GamesPlayed }
            });
            
            var endGameModel = new EndGameModel(Score, PlayerDataService.PlayerData.MaxScore);
            await endGameModel.OpenAndShow("EndGameWindow", Bootstrapper.SessionToken);
            await Event<UiHideEvent>.Wait(Bootstrapper.SessionToken);
            using var mute = Mute();
            await PlatformService.ShowInterstitial(Bootstrapper.SessionToken);
            await Fsm.Enter(new CoreState(), Bootstrapper.SessionToken);
        }

        private IDisposable Mute()
        {
            if (PlayerDataService.PlayerData.MusicEnabled)
            {
                SoundService.Mute();
            }
            
            return new Disposable(() =>
            {
                if (PlayerDataService.PlayerData.MusicEnabled)
                {
                    SoundService.UnMute();
                }
            });
        }

        #region Cheats

        private CheatLabel _cheatLabel;
        private CheatButton _cheatEndGameButton;
        
        void ICheatsProvider.OnGUI()
        {
            _cheatLabel.OnGUI();
            _cheatEndGameButton.OnGUI();
        }

        string ICheatsProvider.Id => "Board";
        #endregion
    }
}