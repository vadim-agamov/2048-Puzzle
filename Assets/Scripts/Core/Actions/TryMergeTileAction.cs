using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;
using Modules.Actions;
using Vector2Int = UnityEngine.Vector2Int;

namespace Core.Actions
{
    public class TryMergeTileAction : ActionBase
    {
        public TryMergeTileAction(BoardModel model, BoardView view, Vector2Int position)
        {
            var context = new Context();
            LogicAction = new Logic(model, position, context);
            if (view != null)
            {
                VisualAction = new Visual(view, context);
            }
        }
        
        private class Context
        {
            public TileModel TileToRemoveA;
            public TileModel TileToRemoveB;
            public TileModel TileToAdd;
        }

        private class Logic : LogicActionBase
        {
            private BoardModel Model { get; }
            private Vector2Int Position { get; }
            private Context Context { get; }

            public Logic(BoardModel model, Vector2Int position, Context context)
            {
                Model = model;
                Position = position;
                Context = context;
            }

            public override bool Do()
            {
                return TryMergeWith(Position + Vector2Int.right) ||
                       TryMergeWith(Position + Vector2Int.left) ||
                       TryMergeWith(Position + Vector2Int.up) ||
                       TryMergeWith(Position + Vector2Int.down);


                bool TryMergeWith(Vector2Int other)
                {
                    if (other.x < 0 || other.x > Model.Size.x - 1 || other.y < 0 || other.y > Model.Size.y - 1)
                    {
                        return false;
                    }

                    var otherModel = Model.Tiles[other.x, other.y];
                    if (otherModel == null)
                    {
                        return false;
                    }
                    
                    var tileModel = Model.Tiles[Position.x, Position.y];
                    if (otherModel.Type != tileModel.Type)
                    {
                        return false;
                    }
                    
                    Context.TileToRemoveA = tileModel;
                    Model.Tiles[Position.x, Position.y] = null;

                    Context.TileToRemoveB = otherModel;
                    Context.TileToAdd = new TileModel(tileModel.Type.Next(), other);
                    Model.Tiles[other.x, other.y] = Context.TileToAdd;
                    
                    return true;
                }
            }
        }

        private class Visual : VisualActionBase
        {
            private BoardView BoardView { get; }
            private Context Context { get; }
            
            public Visual(BoardView view, Context context)
            {
                BoardView = view;
                Context = context;
            }

            public override UniTask Do()
            {
                BoardView.RemoveTile(Context.TileToRemoveA);
                BoardView.RemoveTile(Context.TileToRemoveB);
                
                BoardView.CreateTile(Context.TileToAdd);
                return UniTask.CompletedTask;
            }
        }
    }
}