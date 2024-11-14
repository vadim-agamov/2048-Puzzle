using System.Linq;
using System.Threading;
using Core.Actions;
using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;
using Modules.Actions;
using Modules.AnalyticsService;
using Modules.Extensions;
using Modules.Initializator;
using Modules.ServiceLocator;
using Services.GamePlayerDataService;
using UnityEngine;

namespace Core.Controller
{
    public class BoardController : IInitializable
    {
        private readonly BoardView _boardView;
        private readonly BoardModel _boardModel;

        private IAnalyticsService AnalyticsService { get; set; }
        private GamePlayerDataService PlayerDataService { get; set; }

        [Inject]
        private void Initialize(IAnalyticsService analyticsService, GamePlayerDataService gamePlayerDataService)
        {
            AnalyticsService = analyticsService;
            PlayerDataService = gamePlayerDataService;
        }

        public BoardController(BoardView boardView)
        {
            _boardView = boardView;
            _boardModel = new BoardModel(new Vector2Int(4, 4));
            _boardModel.Hand.SetTile(0, new TileModel(TileType.Tile1, 0));
            _boardModel.Hand.SetTile(1, new TileModel(TileType.Tile1, 1));
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
                        .Add(new TryBestMergeTileAction(_boardModel, _boardView, tileView.Model))
                        .Do();
                    
                    CheckEndGame();
                }
                else
                {
                    tileView.RestoreHandPosition();
                }
                
                Debug.Log($"{_boardModel.Score}");
            }
        }

        private void CheckEndGame()
        {
            if (!_boardModel.Tiles.Where(m => m == null).Any())
            {
                Debug.Log("End game");
                return;
            }
        }

        public async UniTask Initialize(CancellationToken cancellationToken)
        {
            _boardView.Initialize(this, _boardModel);
            await UniTask.Delay(1000, cancellationToken: cancellationToken);
            IsInitialized = true;
        }

        public bool IsInitialized { get; private set; }
    }
}