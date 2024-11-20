using System;
using System.Collections.Generic;
using Core.Configs;
using Core.Controller;
using Core.Models;
using Cysharp.Threading.Tasks;
using Modules.ComponentWithModel;
using Modules.Extensions;
using Modules.Utils;
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
        private PlayAdTileView _playAdTilePrefab;
        
        [SerializeField]
        private TilesConfig _tilesConfig;

        private TileView[] _tileViews;
        private HandModel Model { get; set; }
        private BoardView BoardView { get; set; }
        private BoardController Controller { get; set; }
        
        private IPooledGameObject _playAdTileView;
        private readonly List<IPooledGameObject> _emptyCells = new List<IPooledGameObject>();
        private bool SlotForAdTileEnabled => Model.Size < BoardView.TileCells.GetLength(0);

        private IEnumerable<IPooledGameObject> PooledObjects()
        {
            yield return _playAdTileView;
            foreach (var cell in _emptyCells)
            {
                yield return cell;
            }
            foreach (var tile in _tileViews)
            {
                if (tile == null)
                {
                    continue;
                }
                yield return tile;
            }   
        }

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

        public void Initialize(HandModel model, BoardView boardView, BoardController boardController)
        {
            BoardView = boardView;
            Model = model;
            Controller = boardController;
            CreateCells();
            CreateTiles();
            CreatePlayAdTileView();
        }
        
        public void Release()
        {
            foreach (var pooledObject in PooledObjects())
            {
                BoardView.TilePool.Release(pooledObject);
            }
            
            _emptyCells.Clear();
            _playAdTileView = null;
            _tileViews = null;
        }

        private void CreatePlayAdTileView()
        {
            if (!SlotForAdTileEnabled)
            {
                return;
            }
            
            var playAdTileView = BoardView.TilePool.Get(_playAdTilePrefab);
            playAdTileView.transform.SetParent(_tilesContainer);
            playAdTileView.transform.localPosition = GetCellPosition(Model.Size);
            playAdTileView.OnClick += () => Controller.AddSlotToHand().Forget();
            _playAdTileView = playAdTileView;
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
                var cell =  BoardView.TilePool.Get(_emptyCellPrefab);
                cell.transform.SetParent(_gridContainer);
                cell.transform.localPosition = GetCellPosition(i);
                _emptyCells.Add(cell);
            }
        }

        private Vector3 GetCellPosition(int index)
        {
            var totalCells = SlotForAdTileEnabled ? Model.Size : Model.Size - 1; // 1 for ad tile
            var handWidth = totalCells * _grid.cellSize.x;
            var cellPosition = Mathf.Lerp(-handWidth / 2, handWidth / 2, index / (float)totalCells);
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