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
using Modules.DiContainer;
using Modules.Events;
using Modules.Extensions;
using Modules.FlyItemsService;
using Modules.Fsm;
using Modules.Initializator;
using Modules.PlatformService;
using Modules.SoundService;
using Modules.UIService;
using Modules.UIService.Events;
using Modules.Utils;
using Services.GamePlayerDataService;
using UI;
using UnityEngine;
using Void = Modules.Actions.Void;

#if DEV
using Modules.CheatService;
using Modules.CheatService.Controls;
#endif

namespace Core.Controller
{
    public class BoardController : IInitializable
#if DEV
        , ICheatsProvider
#endif
    {
        private readonly BoardView _boardView;
        private BoardModel BoardModel => PlayerDataService.PlayerData.BoardModel;
        private bool _addIsInProgress;
        private int _slotsAdded = 0;
        private IAnalyticsService AnalyticsService { get; set; }
        private GamePlayerDataService PlayerDataService { get; set; }
        private IFlyItemsService FlyItemsService { get; set; }
        private UIService UiService { get; set; }
        private IPlatformService PlatformService { get; set; }
        private ISoundService SoundService { get; set; }
        public int Score => BoardModel?.Score ?? 0;
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

            var size = PlayerDataService.PlayerData.MaxScore switch
            {
                < 10000 => new Vector2Int(4, 4),
                < 15000 => new Vector2Int(4, 5),
                < 25000 => new Vector2Int(5, 5),
                < 35000 => new Vector2Int(5, 6),
                < 45000 => new Vector2Int(6, 6),
                _ => new Vector2Int(6, 7)
            };
            PlayerDataService.PlayerData.BoardModel ??= new BoardModel(size);
        }

        public BoardController(BoardView boardView) => _boardView = boardView;

        public UniTask Initialize(CancellationToken cancellationToken)
        {
            BoardModel.Hand.SetTile(0, new TileModel(TileType.Tile1, 0));
            _boardView.Initialize(this, BoardModel, UiService);

#if DEV
            _cheatLabel = new CheatLabel(() => $"Score: {Score}");
            _cheatEndGameButton = new CheatButton("EndGame", () => EndGame().Forget());
#endif

            IsInitialized = true;

            AnalyticsService.TrackEvent("begin_game", new Dictionary<string, object>
            {
                { "board_size", $"{BoardModel.Size.x}x{BoardModel.Size.y}" },
                { "games_played", PlayerDataService.PlayerData.GamesPlayed }
            });

            return UniTask.CompletedTask;
        }


        public void PutTileOnBoard(Vector2Int position, TileView tileView)
        {
            Do().Forget();

            async UniTask Do()
            {
                var result = await new PutBlockOnBoardAction(BoardModel, _boardView, position, tileView).Do(Result<Void>.Succeed(default));

                if (result.Success)
                {
                    var action = new ParallelAction<TileModel>()
                        .Add(
                            SequenceAction
                                .Start(new TryBestMergeTileAction(BoardModel, _boardView, FlyItemsService).SuppressResult())
                                .Append(new SpawnTileOnBoardAction(BoardModel, _boardView))
                                .Append(new TryBestMergeTileAction(BoardModel, _boardView, FlyItemsService)))
                        .Add(new TryRefillHandAction(BoardModel, _boardView));
                    
                    await action.Do(result);
                    
                    CheckEndGame();
                }
                else
                {
                    tileView.RestoreHandPosition();
                }

                PlayerDataService.Commit();
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
                    BoardModel.Hand.IncreaseSize();
                    _boardView.ReloadHand();
                    await new TryRefillHandAction(BoardModel, _boardView).Do(Result<Void>.Succeed(default));
                    PlayerDataService.Commit();
                }
            }
            finally
            {
                _addIsInProgress = false;
            }
        }

        private void CheckEndGame()
        {
            if (BoardModel.Tiles.Where(m => m == null).Any())
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
                { "board_size", $"{BoardModel.Size.x}x{BoardModel.Size.y}" },
                { "games_played", PlayerDataService.PlayerData.GamesPlayed }
            });

            var endGameModel = new EndGameModel(Score, PlayerDataService.PlayerData.MaxScore);
            PlayerDataService.PlayerData.BoardModel = null;

            await endGameModel.OpenAndShow("EndGameWindow", Bootstrapper.SessionToken);
            await Event<UiHideEvent>.Wait(Bootstrapper.SessionToken);
            using var mute = Mute();
            await PlatformService.ShowInterstitial(Bootstrapper.SessionToken);
            await Fsm.Enter(new CoreState(), Bootstrapper.SessionToken);
        }

        public async UniTask Restart()
        {
            PlayerDataService.PlayerData.GamesPlayed++;
            PlayerDataService.ForceCommit();
            AnalyticsService.TrackEvent("restart_game", new Dictionary<string, object>
            {
                { "score", Score },
                { "slots_added", _slotsAdded },
                { "board_size", $"{BoardModel.Size.x}x{BoardModel.Size.y}" },
                { "games_played", PlayerDataService.PlayerData.GamesPlayed }
            });

            PlayerDataService.PlayerData.BoardModel = null;
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

#if DEV

        private CheatLabel _cheatLabel;
        private CheatButton _cheatEndGameButton;

        void ICheatsProvider.OnGUI()
        {
            _cheatLabel.OnGUI();
            _cheatEndGameButton.OnGUI();
        }

        string ICheatsProvider.Id => "Board";
#endif
    }
}