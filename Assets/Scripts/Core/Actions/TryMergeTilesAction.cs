using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;
using Modules.Actions;
using UnityEngine;

namespace Core.Actions
{
    public class TryMergeTilesAction : IAction
    {
        private readonly BoardModel _model;
        private readonly BoardView _view;

        public TryMergeTilesAction(BoardModel model, BoardView view)
        {
            _model = model;
            _view = view;
        }

        public async UniTask<bool> Do()
        {
            var result = false;

            while (true)
            { 
                var merged = await TryMergeTile();
                result |= merged;
                if(!merged)
                { 
                    break;
                }
            }

            return result;
        }

        private async UniTask<bool> TryMergeTile()
        {
            for (var x = 0; x < _model.Size.x; x++)
            {
                for (var y = 0; y < _model.Size.y; y++)
                {
                    if (_model.Tiles[x, y] == null)
                    {
                        continue;
                    }

                    var success = await new TryMergeTileAction(_model, _view, new Vector2Int(x, y)).Do();
                    if (success)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}