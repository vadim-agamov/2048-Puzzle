using System.Linq;
using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;
using Modules.Actions;
using Modules.Extensions;
using UnityEngine;

namespace Core.Actions
{
    public class SpawnTileOnBoardAction : ActionBase<Void,TileModel>
    {
        public SpawnTileOnBoardAction(BoardModel model, BoardView view)
        {
            var context = new Context();
            LogicAction = new SpawnBlockOnBoardLogicAction(model, context);
            if (view != null)
            {
                VisualAction = new SpawnBlockOnBoardVisualAction(view, context);
            }
        }
        
        private class Context
        {
            public TileModel TileModel;
        }
        
        private class SpawnBlockOnBoardLogicAction : LogicActionBase<Void,TileModel>
        {
            private readonly BoardModel _model;
            private readonly Context _context;

            public SpawnBlockOnBoardLogicAction(BoardModel model, Context context)
            {
                _model = model;
                _context = context;
            }

            public override Result<TileModel> Do(Result<Void> _)
            {
                var freeCells = _model.Tiles
                    .Select((v,x,y) => new {v,x,y})
                    .Where(item => item.v == null)
                    .Select(item => new Vector2Int(item.x, item.y))
                    .ToArray();
                
                if (freeCells.Length == 0)
                {
                    return Result<TileModel>.Failed();
                }

                var newTile = new TileModel(GetRandomTileType(),freeCells.Random());
                _context.TileModel = newTile;
                _model.Tiles[newTile.BoardPosition.x, newTile.BoardPosition.y] = newTile;
                return Result<TileModel>.Succeed(newTile);
            }

            private TileType GetRandomTileType()
            {
                var maxTileOnBoard = _model.Tiles
                    .Where(m => m != null)
                    .Where(m => m.Type != TileType.None)
                    .Where(m => m.Type != TileType.MaxTile)
                    .Select(m => (int)m.Type)
                    .Append((int)TileType.Tile1)
                    .Max();

                const int minTile = (int)TileType.Tile1;
                var options = Enumerable.Range(minTile, 1 + maxTileOnBoard - minTile).Cast<TileType>().ToArray();
                var weights = options.Select((_,index) => 1 / (float)(index * index + 1)).ToArray();
                return options.Random(weights);
            }
        }
        
        private class SpawnBlockOnBoardVisualAction : VisualActionBase
        {
            private readonly BoardView _boardView;
            private readonly Context _context;

            public SpawnBlockOnBoardVisualAction(BoardView boardView, Context context)
            {
                _boardView = boardView;
                _context = context;
            }

            public override UniTask Do()
            {
                return _boardView.CreateTile(_context.TileModel).Appear();
            }
        }
    }
}