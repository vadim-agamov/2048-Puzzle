using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;
using Modules.Actions;
using Modules.FlyItemsService;
using UnityEngine;

namespace Core.Actions
{
    public class TryBestMergeTileAction : IAction
    {
        private readonly LogicActionBase _logicAction;
        private readonly VisualActionBase _visualAction;
        private readonly BoardModel _model;
        private readonly BoardView _view;
        private readonly Context _context;
        private readonly IFlyItemsService _flyItemsService;

        public TryBestMergeTileAction(BoardModel model, BoardView view, TileModel tileModel, IFlyItemsService flyItemsService)
        {
            _model = model;
            _view = view;
            _context = new Context();
            _flyItemsService = flyItemsService;

            _logicAction = new Logic(_model, tileModel, _context);
            if (view != null)
            {
                _visualAction = new Visual(_model, _view, _context, _flyItemsService);
            }
        }

        public async UniTask<bool> Do()
        {
            var success = _logicAction.Do();
            if (success)
            {
                await _visualAction.Do();
                var tileModel = _model.Tiles[_context.ToPosition.x, _context.ToPosition.y];
                success |= await new TryBestMergeTileAction(_model, _view, tileModel, _flyItemsService).Do();
            }

            return success;
        }

        private class Context
        {
            public Vector2Int ToPosition;
            public Vector2Int FromPosition;
        }

        private class Logic : LogicActionBase
        {
            private readonly Context _context;
            private readonly TileModel _tileModel;
            private readonly BoardModel _model;

            public Logic(BoardModel model, TileModel tileModel, Context context)
            {
                _model = model;
                _tileModel = tileModel;
                _context = context;
            }

            public override bool Do()
            {
                if(_tileModel == null)
                {
                    Debug.LogWarning($"[{nameof(TryBestMergeTileAction)}] TileModel is null");
                    return false;
                }
                
                var success = new BestMergeTileFinder(_model, _tileModel).TryFind(out _context.FromPosition, out _context.ToPosition);
                if (success)
                {
                    var toTileModel = _model.Tiles[_context.ToPosition.x, _context.ToPosition.y];
                    var nextTileModel = new TileModel(toTileModel.Type.Next(), _context.ToPosition);
                    
                    _model.Tiles[_context.FromPosition.x, _context.FromPosition.y] = null;
                    _model.Tiles[_context.ToPosition.x, _context.ToPosition.y] = nextTileModel;
                    _model.AddScore(nextTileModel.Type.Score());
                }

                return success;
            }
        }

        private class Visual : VisualActionBase
        {
            private readonly BoardView _view;
            private readonly Context _context;
            private readonly BoardModel _model;
            private readonly IFlyItemsService _flyItemsService;

            public Visual(BoardModel model, BoardView view, Context context, IFlyItemsService flyItemsService)
            {
                _model = model;
                _view = view;
                _context = context;
                _flyItemsService = flyItemsService;
            }

            public override async UniTask Do()
            {
                var tileViewFrom = _view.TileCells[_context.FromPosition.x, _context.FromPosition.y];
                var tileViewTo = _view.TileCells[_context.ToPosition.x, _context.ToPosition.y];
                
                var fromScreenPosition = Camera.main.WorldToScreenPoint(tileViewFrom.transform.position);
                var score = _model.Tiles[_context.ToPosition.x, _context.ToPosition.y].Type.Score();

                _view.RemoveTile(_context.FromPosition);
                _view.RemoveTile(_context.ToPosition);

                _flyItemsService.Fly("ScoreToken", fromScreenPosition, "score", score, FlyType.FlyUp).Forget();
                
                await (
                    tileViewFrom.MoveTo(tileViewTo.transform.position),
                    tileViewTo.Disappear(),
                    _view.CreateTile(_model.Tiles[_context.ToPosition.x, _context.ToPosition.y]).Appear()
                    );
                
                _view.ReleaseTile(tileViewFrom);
                _view.ReleaseTile(tileViewTo);
            }
        }
    }
}