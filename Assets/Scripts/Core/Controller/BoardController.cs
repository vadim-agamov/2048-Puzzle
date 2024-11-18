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
using Modules.Extensions;
using Modules.FlyItemsService;
using Modules.FSM;
using Modules.Initializator;
using Modules.ServiceLocator;
using Modules.UIService;
using Services.GamePlayerDataService;
using UnityEngine;

namespace Core.Controller
{
    public class BoardController : IInitializable, ICheatsProvider
    {
        private readonly BoardView _boardView;
        private readonly BoardModel _boardModel;
        private string _id;

        private IAnalyticsService AnalyticsService { get; set; }
        private GamePlayerDataService PlayerDataService { get; set; }
        private IFlyItemsService FlyItemsService { get; set; }
        public UIService UiService { get; set; }
        public int Score => _boardModel.Score;


        [Inject]
        private void Initialize(IAnalyticsService analyticsService, GamePlayerDataService gamePlayerDataService, IFlyItemsService flyItemsService, UIService uiService)
        {
            AnalyticsService = analyticsService;
            PlayerDataService = gamePlayerDataService;
            FlyItemsService = flyItemsService;
            UiService = uiService;
        }

        public BoardController(BoardView boardView)
        {
            _boardView = boardView;
            _boardModel = new BoardModel(new Vector2Int(4, 4));
            _boardModel.Hand.SetTile(0, new TileModel(TileType.Tile1, 0));
            _boardModel.Hand.SetTile(1, new TileModel(TileType.Tile1, 1));

            _cheatLabel = new CheatLabel(() => $"Score: {Score}");
            _cheatEndGameButton = new CheatButton("EndGame", EndGame);
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

        private void CheckEndGame()
        {
            if (_boardModel.Tiles.Where(m => m == null).Any())
            {
                return;
            }
            
            EndGame();
        }

        private void EndGame()
        {
            PlayerDataService.PlayerData.MaxScore = Score;
            PlayerDataService.Commit();
            Fsm.Enter(new CoreState(), Bootstrapper.SessionToken).Forget();
        }

        public async UniTask Initialize(CancellationToken cancellationToken)
        {
            _boardView.Initialize(this, _boardModel, UiService);
            await UniTask.Delay(1000, cancellationToken: cancellationToken);
            IsInitialized = true;
        }

        public bool IsInitialized { get; private set; }
        
        
        #region Cheats

        private readonly CheatLabel _cheatLabel;
        private readonly CheatButton _cheatEndGameButton;
        private ICheatService CheatService { get; set; }
        
        void ICheatsProvider.OnGUI()
        {
            _cheatLabel.OnGUI();
            _cheatEndGameButton.OnGUI();
        }

        string ICheatsProvider.Id => "Board";
        #endregion
    }
}