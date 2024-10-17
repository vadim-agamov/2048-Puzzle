using Core.Controller;
using Core.Models;
using Cysharp.Threading.Tasks;
using Modules.Extensions;
using Modules.UiComponents;
using UnityEngine;

namespace Core.Views
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField]
        private Camera _camera;
        
        [SerializeField] 
        private Grid _grid;

        [SerializeField] 
        private Transform _gridContainer;
        
        [SerializeField] 
        private Transform _cellsContainer;

        [SerializeField] 
        private TileView _tileViewPrefab;
        
        [SerializeField]
        private EmptyCellView _emptyCellPrefab;

        [SerializeField] 
        private HandView _handView;

        private EmptyCellView[,] _gridCells;
        private TileView[,] _tileCells;
        private Vector2Int _hoveredCell = Vector2Int.zero;
        
        private BoardModel Model { get; set; }
        private BoardController Controller { get; set; }
        
        public void Initialize(BoardController controller, BoardModel model)
        {
            Model = model;
            Controller = controller;
            CreateGrid();
            CreateCells();
            CreateHand(model);
            FitCamera();
        }

        public void OnTileDrag(TileView tileView)
        {
            var cell = (Vector2Int)_grid.WorldToCell(tileView.transform.position);
            if (cell.x < 0 || cell.x > Model.Size.x - 1 ||
                cell.y < 0 || cell.y > Model.Size.y - 1)
            {
                return;
            }   
            
            if (_hoveredCell != cell) 
            {
                // Debug.Log($"{_hoveredGrid.x}-{_hoveredGrid.y}");
                _gridCells[_hoveredCell.x, _hoveredCell.y].HoverOut();
                _hoveredCell = cell;
                _gridCells[_hoveredCell.x, _hoveredCell.y].HoverIn();
            }
        }
        

        public void OnTileDragEnd(TileView tileView)
        {
            var cell = (Vector2Int)_grid.WorldToCell(tileView.transform.position);
            if (cell.x < 0 || cell.x > Model.Size.x - 1 ||
                cell.y < 0 || cell.y > Model.Size.y - 1)
            {
                tileView.RestorePosition();
                return;
            }  
            
            Controller.PutTileOnBoard(cell, tileView);
        }
        
        private void CreateHand(BoardModel model)
        {
            _handView.SetModel(model.Hand, this);
            var rect = VisibleGridWorldRect;
            _handView.transform.position = new Vector3(rect.center.x, rect.yMin - _grid.cellSize.y);
            
            foreach (var handViewTileView in _handView.TileViews)
            {
                if(handViewTileView == null)
                    continue;
            }
        }
        
        private void CreateGrid()
        {
            _gridCells = new EmptyCellView[Model.Size.x, Model.Size.y];
            for (var x = 0; x < Model.Size.x; x++)
            {
                for (var y = 0; y < Model.Size.y; y++)
                {
                    var tile = Instantiate(_emptyCellPrefab, _gridContainer);
                    tile.transform.position = _grid.GetCellCenterWorld(new Vector3Int(x, y));
                    _gridCells[x, y] = tile;
                }
            }
        }

        private void CreateCells()
        {
            _tileCells = new TileView[Model.Size.x, Model.Size.y];
            for (var x = 0; x < Model.Size.x; x++)
            {
                for (var y = 0; y < Model.Size.y; y++)
                {
                    var model = Model.Tiles[x, y];
                    if (model == null || model.Type == TileType.None)
                    {
                        continue;
                    }

                    var tile = model.CreateView(_tileViewPrefab, _cellsContainer, this);
                    tile.transform.position = _grid.GetCellCenterWorld(new Vector3Int(x, y));
                    _tileCells[x, y] = tile;
                }
            }
        }
        
        public UniTask PutTile(Vector2Int position, TileView tileView)
        {
            Debug.Assert(_tileCells[position.x, position.y] == null, $"Tile already exists {position}");
            
            tileView.transform.position = _grid.GetCellCenterWorld(new Vector3Int(position.x, position.y));
            tileView.transform.SetParent(_cellsContainer);
            tileView.IsDraggable = false;
            _tileCells[position.x, position.y] = tileView;
            _handView.RemoveTile(tileView);
            return UniTask.CompletedTask;
        }

        private void FitCamera() => _camera.FitOrthographic(VisibleWorldRect, 1);

        private Rect VisibleWorldRect => VisibleGridWorldRect.Union(_handView.VisibleWorldRect);

        private Rect VisibleGridWorldRect
        {
            get
            {
                var cellSize  = _grid.cellSize;
                return new Rect
                {
                    min = _grid.GetCellCenterWorld(Vector3Int.zero) - cellSize / 2,
                    max = _grid.GetCellCenterWorld(new Vector3Int(Model.Size.x - 1, Model.Size.y - 1)) + cellSize / 2
                };
            }
        }
        
        private void OnDrawGizmos()
        {
            if (Model == null)
            {
                return;
            }

            var rect = VisibleWorldRect;
            Gizmos.DrawLineList(new[]
            {
                new Vector3(rect.x, rect.y), new Vector3(rect.x + rect.width, rect.y),
                new Vector3(rect.x + rect.width, rect.y), new Vector3(rect.x + rect.width, rect.y + rect.height),
                new Vector3(rect.x + rect.width, rect.y + rect.height), new Vector3(rect.x, rect.y + rect.height),
                new Vector3(rect.x, rect.y + rect.height), new Vector3(rect.x, rect.y)
            });
        }
    }
}