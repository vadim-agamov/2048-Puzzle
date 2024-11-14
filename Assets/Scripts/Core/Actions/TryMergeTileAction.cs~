using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;
using Modules.Actions;
using Vector2Int = UnityEngine.Vector2Int;

namespace Core.Actions
{
    public class TryMergeTileAction : IAction
    {
        private BoardModel Model { get; }
        private Vector2Int Position { get; }
        private BoardView View { get; }

        public TryMergeTileAction(BoardModel model, BoardView view, Vector2Int position)
        {
            Model = model;
            Position = position;
            View = view;
        }

        public async UniTask<bool> Do()
        {
            var success = TryMergeLogical(Position, out var other);
            if (success)
            {
                await MergeVisual(other);
                success |= await new TryMergeTileAction(Model, View, other).Do();
            }

            return success;
        }

        private async UniTask MergeVisual(Vector2Int other)
        {
            var tileViewA = View.TileCells[Position.x, Position.y];
            var tileViewB = View.TileCells[other.x, other.y];

            await tileViewA.MoveTo(tileViewB.transform.position);

            View.RemoveTile(Position);
            View.RemoveTile(other);
            View.CreateTile(Model.Tiles[other.x, other.y]);
        }

        private bool TryMergeLogical(Vector2Int position, out Vector2Int other)
        {
            return TryMergeWith(position + Vector2Int.right, out other) ||
                   TryMergeWith(position + Vector2Int.left, out other) ||
                   TryMergeWith(position + Vector2Int.up, out other) ||
                   TryMergeWith(position + Vector2Int.down, out other);

            bool TryMergeWith(Vector2Int other, out Vector2Int result)
            {
                result = other;
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

                Model.Tiles[Position.x, Position.y] = null;
                Model.Tiles[other.x, other.y] = new TileModel(tileModel.Type.Next(), other);

                return true;
            }
        }
    }
}