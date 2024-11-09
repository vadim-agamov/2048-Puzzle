using System;
using System.Collections.Generic;
using System.Linq;
using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;
using Modules.Actions;
using Modules.Extensions;

namespace Core.Actions
{
    public class TryRefillHandAction : ActionBase
    {
        public TryRefillHandAction(BoardModel boardModel, BoardView boardView)
        {
            var context = new Context();
            LogicAction = new Logic(boardModel, context);
            if (boardView != null)
            {
                VisualAction = new Visual(boardModel, boardView, context);
            }
        }

        private class Context
        {
            public int[] RefilledTiles;
        }

        private class Logic : LogicActionBase
        {
            private readonly BoardModel _boardModel;
            private readonly Context _context;

            public Logic(BoardModel boardModel, Context context)
            {
                _boardModel = boardModel;
                _context = context;
            }

            public override bool Do()
            {
                if(_boardModel.Hand.Tiles.Any(m => m != null))
                {
                    return false;
                }
                
                var result = false;
                var refilledTiles = new List<int>();
                for(var i = 0; i < _boardModel.Hand.Size; i++)
                {
                    if(_boardModel.Hand.Tiles[i] == null)
                    {
                        result = true;
                        var maxTileOnBoard = _boardModel.Tiles
                            .Where((m, _, _) => m != null)
                            .Where(m => m.Type != TileType.None)
                            .Select(m => (int)m.Type)
                            .Max();

                        var minTile = (int)TileType.Tile1;// Math.Max(1, maxTileOnBoard - 5);
                        var maxTile = Math.Max(1, maxTileOnBoard - 2);
                        var tile = (TileType)UnityEngine.Random.Range(minTile, maxTile + 1);
                        
                        _boardModel.Hand.SetTile(i, new TileModel(tile, i));
                        
                        refilledTiles.Add(i);
                    }
                }
                
                _context.RefilledTiles = refilledTiles.ToArray();
                return result;
            }
        }

        private class Visual : VisualActionBase
        {
            private BoardModel Model { get; }

            private BoardView View { get;}
            
            private Context Context { get; }

            public Visual(BoardModel boardModel, BoardView boardView, Context context)
            {
                Model = boardModel;
                View = boardView;
                Context = context;
            }

            public override async UniTask Do()
            {
                foreach (var index in Context.RefilledTiles)
                {
                    await UniTask.Delay(100);
                    View.HandView.CreateTile(Model.Hand.Tiles[index], index);
                }
            }
        }
    }
}