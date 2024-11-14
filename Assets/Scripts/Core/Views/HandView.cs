using System;
using Core.Configs;
using Core.Models;
using Cysharp.Threading.Tasks;
using Modules.Extensions;
using UnityEngine;

namespace Core.Views
{
    public class HandView : MonoBehaviour
    {
        [SerializeField] 
        private Grid _grid;
        
        [SerializeField]
        private Transform _gridContainer;

        [SerializeField]
        private Transform _tilesContainer;
        
        [SerializeField]
        private EmptyCellView _emptyCellPrefab;
        
        [SerializeField]
        private TilesConfig _tilesConfig;

        private TileView[] _tileViews;
        private HandModel Model { get; set; }
        private BoardView BoardView { get; set; }

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

        public void Initialize(HandModel model, BoardView boardView)
        {
            BoardView = boardView;
            Model = model;
            CreateCells();
            CreateTiles();
        }

        private void CreateTiles()
        {
            _tileViews = new TileView[Model.Size];
            for (var i = 0; i < Model.Size; i++)
            {
                if (Model.Tiles[i] == null)
                {
                    continue;
                }
                
                CreateTile(Model.Tiles[i], i).Idle().Forget();
            }
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

        public TileView[] Tiles => _tileViews;
        
        public void RemoveTile(TileView tileView)
        {
            var index = Array.IndexOf(_tileViews, tileView);
            _tileViews[index] = null;
        }

        public TileView CreateTile(TileModel model, int i)
        {
            var tile = model.CreateView(BoardView.TilePool.Get, _tilesConfig.GetPrefab(model.Type), _tilesContainer, BoardView);
            tile.transform.localPosition = GetCellPosition(i);
            tile.IsDraggable = true;
            _tileViews[i] = tile;
            return tile;
        }
    }
}