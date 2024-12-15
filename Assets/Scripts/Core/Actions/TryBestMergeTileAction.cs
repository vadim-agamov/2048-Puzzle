using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;
using Modules.Actions;
using Modules.FlyItemsService;
using UnityEngine;

namespace Core.Actions
{
    public class TryBestMergeTileAction : IAction<TileModel, TileModel>
    {
        private readonly LogicActionBase<TileModel, TileModel> _logicAction;
        private readonly VisualActionBase _visualAction;
        private readonly BoardModel _model;
        private readonly BoardView _view;
        private readonly Context _context;
        private readonly IFlyItemsService _flyItemsService;

        public IAction<TileModel, Void> SuppressResult() => new SuppressResultAction<TileModel,TileModel>(this);

        public TryBestMergeTileAction(BoardModel model, BoardView view,  IFlyItemsService flyItemsService)
        {
            _model = model;
            _view = view;
            _context = new Context();
            _flyItemsService = flyItemsService;

            _logicAction = new Logic(_model, _context);
            if (view != null)
            {
                _visualAction = new Visual(_model, _view, _context, _flyItemsService);
            }
        }
        
        public async UniTask<Result<TileModel>> Do(Result<TileModel> input)
        { 
            if(!input.Success)
            {
                return Result<TileModel>.Failed();
            }
            
            var result = _logicAction.Do(input);
            if (result.Success)
            {
                await _visualAction.Do();
                var r = await new TryBestMergeTileAction(_model, _view, _flyItemsService).Do(result);
                if(r.Success)
                {
                    result = r;
                }
            }

            return result;
        }

        private class Context
        {
            public Vector2Int ToPosition;
            public Vector2Int FromPosition;
        }

        private class Logic : LogicActionBase<TileModel, TileModel>
        {
            private readonly Context _context;
            private readonly BoardModel _model;

            public Logic(BoardModel model, Context context)
            {
                _model = model;
                _context = context;
            }

            public override Result<TileModel> Do(Result<TileModel> input)
            {
                if(input.Value == null)
                {
                    Debug.LogWarning($"[{nameof(TryBestMergeTileAction)}] TileModel is null");
                    return Result<TileModel>.Failed();
                }
                
                var success = new BestMergeTileFinder(_model, input.Value).TryFind(out _context.FromPosition, out _context.ToPosition);
                if (success)
                {
                    var toTileModel = _model.Tiles[_context.ToPosition.x, _context.ToPosition.y];
                    var nextTileModel = new TileModel(toTileModel.Type.Next(), _context.ToPosition);
                    
                    _model.Tiles[_context.FromPosition.x, _context.FromPosition.y] = null;
                    _model.Tiles[_context.ToPosition.x, _context.ToPosition.y] = nextTileModel;
                    _model.AddScore(nextTileModel.Type.Score());
                    
                    return Result<TileModel>.Succeed(nextTileModel);
                }

                return Result<TileModel>.Failed();
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