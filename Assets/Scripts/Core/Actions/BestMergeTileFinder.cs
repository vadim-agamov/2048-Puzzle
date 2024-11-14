using System.Collections.Generic;
using Core.Models;
using UnityEngine;

namespace Core.Actions
{
    public class BestMergeTileFinder
    {
        private TileModel _tileFrom;
        private TileModel _tileTo;
        private int _length;
        private readonly BoardModel _boardModel;
        private readonly LinkedList<TileModel> _chain = new();


        public BestMergeTileFinder(BoardModel model, TileModel tileFrom)
        {
            _boardModel = model;
            _tileFrom = tileFrom;
        }

        public bool TryFind(out Vector2Int tileFrom, out Vector2Int tileTo)
        {
            Find(_tileFrom);
            tileTo = _tileTo?.BoardPosition ?? Vector2Int.zero;
            tileFrom = _tileFrom?.BoardPosition ?? Vector2Int.zero;
            return _length > 0;
        }

        private void Find(TileModel tile)
        {
            foreach (var other in GetNeighbors(tile))
            {
                if (CanMerge(tile, other))
                {
                    _chain.AddLast(tile);
                    _chain.AddLast(other);
                    StoreLongestChain();
                    Find(new TileModel(other.Type.Next(), other.BoardPosition));
                    _chain.RemoveLast();
                    _chain.RemoveLast();
                }

                if (CanMerge(other, tile))
                {
                    _chain.AddLast(other);
                    _chain.AddLast(tile);
                    StoreLongestChain();
                    Find(new TileModel(tile.Type.Next(), tile.BoardPosition));
                    _chain.RemoveLast();
                    _chain.RemoveLast();
                }
            }

            void StoreLongestChain()
            {
                if (_chain.Count > _length)
                {
                    _length = _chain.Count;
                    _tileFrom = _chain.First.Value;
                    _tileTo = _chain.First.Next?.Value;
                }
            }
        }

        private static bool CanMerge(TileModel from, TileModel to) => from.Type == to.Type;

        private IEnumerable<TileModel> GetNeighbors(TileModel tile)
        {
            if (IsValid(tile.BoardPosition + Vector2Int.right, out var model))
            {
                yield return model;
            }

            if (IsValid(tile.BoardPosition + Vector2Int.left, out model))
            {
                yield return model;
            }

            if (IsValid(tile.BoardPosition + Vector2Int.up, out model))
            {
                yield return model;
            }

            if (IsValid(tile.BoardPosition + Vector2Int.down, out model))
            {
                yield return model;
            }
            
            if (IsValid(tile.BoardPosition + new Vector2Int(1,1), out model))
            {
                yield return model;
            }
            
            if (IsValid(tile.BoardPosition + new Vector2Int(1,-1), out model))
            {
                yield return model;
            }
            
            if (IsValid(tile.BoardPosition + new Vector2Int(-1,1), out model))
            {
                yield return model;
            }
            
            if (IsValid(tile.BoardPosition + new Vector2Int(-1,-1), out model))
            {
                yield return model;
            }


            bool IsValid(Vector2Int position, out TileModel m)
            {
                m = null;
                if (!IsInBounds(position))
                {
                    return false;
                }

                m = _boardModel.Tiles[position.x, position.y];
                return m != null;
            }

            bool IsInBounds(Vector2Int position) =>
                position.x >= 0 && position.x < _boardModel.Size.x &&
                position.y >= 0 && position.y < _boardModel.Size.y;
        }
    }
}