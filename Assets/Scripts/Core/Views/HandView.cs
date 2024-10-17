using System;
using System.Collections.Generic;
using Core.Models;
using Modules.Extensions;
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
        
        [SerializeField]
        private EmptyCellView _emptyCellPrefab;

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

        public void SetModel(HandModel model, BoardView boardView)
        {
            Model = model;
            CreateCells();
            CreateTiles(boardView);
        }

        private void CreateTiles(BoardView boardView)
        {
            _tileViews = new TileView[Model.Size];
            for (var i = 0; i < Model.Size; i++)
            {
                var tileModel = Model.Tiles[i];
                if (tileModel == null || tileModel.Type == TileType.None)
                {
                    continue;
                }
                
                CreateTile(boardView, tileModel, i);
            }
        }

        private void CreateTile(BoardView boardView, TileModel tileModel, int i)
        {
            var tile = tileModel.CreateView(_tileViewPrefab, _tilesContainer, boardView);
            tile.transform.localPosition = GetCellPosition(i);
            tile.IsDraggable = true;
            _tileViews[i] = tile;
        }

        private void CreateCells()
        {
            for (var i = 0; i < Model.Size; i++)
            {
                var cell = Instantiate(_emptyCellPrefab, _gridContainer);
                cell.transform.localPosition = GetCellPosition(i);
            }
        }

        private Vector3 GetCellPosition(int index)
        {
            var handWidth = (Model.Size - 1) * _grid.cellSize.x;
            var cellPosition = Mathf.Lerp(-handWidth / 2, handWidth / 2, index / (float)(Model.Size - 1));
            return new Vector3(cellPosition, 0, 0);
        }

        public IEnumerable<TileView> TileViews => _tileViews;
        
        public void RemoveTile(TileView tileView)
        {
            var index = Array.IndexOf(_tileViews, tileView);
            _tileViews[index] = null;
        }
    }
}