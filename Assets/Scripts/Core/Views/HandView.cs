using System.Collections.Generic;
using Core.Models;
using UnityEngine;

namespace Core.Views
{
    public class HandView : MonoBehaviour
    {
        [SerializeField] 
        private Grid _grid;

        [SerializeField]
        private TileView _tileViewPrefab;

        [SerializeField]
        private Transform _gridContainer;

        [SerializeField]
        private Transform _tilesContainer;

        private TileView[] _tileViews;
        private HandModel Model { get; set; }

        public Rect VisibleWorldRect
        {
            get
            {
                var first = _gridContainer.GetChild(0).position;
                var last = _gridContainer.GetChild(_gridContainer.childCount - 1).position;
                var rect = new Rect
                {
                    min = first - _grid.cellSize / 2,
                    max = last + _grid.cellSize / 2
                };
                return rect;
            }
        }

        public void SetModel(HandModel model)
        {
            Model = model;

            for (var i = 0; i < Model.Size; i++)
            {
                var cell = Instantiate(_tileViewPrefab, _gridContainer);
                cell.TileType = TileType.None;
                cell.transform.localPosition = GetCellPosition(i);
            }

            _tileViews = new TileView[Model.Size];
            for (var i = 0; i < Model.Size; i++)
            {
                if (Model.Tiles[i] == TileType.None)
                {
                    continue;
                }
                
                var tile = Instantiate(_tileViewPrefab, _tilesContainer);
                tile.TileType = Model.Tiles[i];
                tile.transform.localPosition = GetCellPosition(i);
                _tileViews[i] = tile;
            }
        }

        public IEnumerable<TileView> TileViews => _tileViews;

        private Vector3 GetCellPosition(int index)
        {
            var handWidth = (Model.Size - 1) * _grid.cellSize.x;
            var cellPosition = Mathf.Lerp(-handWidth / 2, handWidth / 2, index / (float)(Model.Size - 1));
            return new Vector3(cellPosition, 0, 0);
        }
    }
}