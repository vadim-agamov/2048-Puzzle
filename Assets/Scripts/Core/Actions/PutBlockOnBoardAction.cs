using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;
using Modules.Actions;
using UnityEngine;

namespace Core.Actions
{
    public class PutBlockOnBoardAction : ActionBase<Void, TileModel>
    {
        public PutBlockOnBoardAction(BoardModel model, BoardView view, Vector2Int position, TileView tileView)
        {
            LogicAction = new PutBlockOnBoardLogicAction(model, position, tileView.Model);
            if (view != null)
            {
                VisualAction = new PutBlockOnBoardVisualAction(model, view, position, tileView);
            }
        }

        private class PutBlockOnBoardLogicAction : LogicActionBase<Void, TileModel>
        {
            private readonly Vector2Int _position;
            private readonly BoardModel _model;
            private readonly TileModel _tileModel;

            public PutBlockOnBoardLogicAction(BoardModel model, Vector2Int position, TileModel tile)
            {
                _model = model;
                _position = position;
                _tileModel = tile;
            }

            public override Result<TileModel> Do(Result<Void> _)
            {
                if (_position.x < 0 ||
                    _position.x >= _model.Size.x ||
                    _position.y < 0 ||
                    _position.y >= _model.Size.y)
                {
                    return Result<TileModel>.Failed();
                }

                if (_model.Tiles[_position.x, _position.y] != null)
                {
                    return Result<TileModel>.Failed();
                }

                var placedModel = _tileModel.WithPosition(_position);
                _model.Tiles[_position.x, _position.y] = placedModel;
                _model.Hand.RemoveTile(_tileModel.HandPosition);
                return Result<TileModel>.Succeed(placedModel);
            }
        }

        private class PutBlockOnBoardVisualAction : VisualActionBase
        {
            private readonly BoardView _boardView;
            private readonly Vector2Int _position;
            private readonly TileView _tileView;
            private readonly BoardModel _boardModel;

            public PutBlockOnBoardVisualAction(BoardModel model, BoardView boardView, Vector2Int position, TileView tileView)
            {
                _boardView = boardView;
                _position = position;
                _tileView = tileView;
                _boardModel = model;
            }

            public override UniTask Do()
            {
                _tileView.UpdateModel(_boardModel.Tiles[_position.x, _position.y]);
                return _boardView.PutTileFromHand(_position, _tileView);
            }
        }
    }
}