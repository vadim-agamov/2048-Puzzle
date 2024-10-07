using Core.Models;
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
        private HandView _handView;

        private TileView[,] _gridCells;
        private TileView[,] _tileCells;
        private Vector2Int _hoveredGrid = Vector2Int.zero;
        
        private BoardModel Model { get; set; }

        public void Start()
        {
            var model = new BoardModel(new Vector2Int(6, 6))
            {
                Tiles =
                {
                    [1, 1] = TileType.Tile1,
                    [4, 4] = TileType.Tile8,
                    [4, 5] = TileType.Tile64,
                }
            };
            model.Hand.SetTile(0, TileType.Tile1);
            model.Hand.SetTile(1, TileType.Tile2);
            model.Hand.SetTile(3, TileType.Tile4);

            SetModel(model);
        }

        public void SetModel(BoardModel model)
        {
            Model = model;
            CreateGrid();
            CreateCells();
            CreateHand(model);
            FitCamera();
        }

        public void OnTileMoved(Vector3 worldPosition)
        {
            var cell = (Vector2Int)_grid.WorldToCell(worldPosition);
            if (cell.x < 0 || cell.x > Model.Size.x - 1 ||
                cell.y < 0 || cell.y > Model.Size.y - 1)
            {
                return;
            }


            if (_hoveredGrid != cell) 
            {
                Debug.Log($"{_hoveredGrid.x}-{_hoveredGrid.y}");
                _gridCells[_hoveredGrid.x, _hoveredGrid.y].HoverOut();
                _hoveredGrid = cell;
                _gridCells[_hoveredGrid.x, _hoveredGrid.y].HoverIn();
            }
        }

        private void CreateHand(BoardModel model)
        {
            _handView.SetModel(model.Hand);
            var rect = VisibleGridWorldRect;
            _handView.transform.position = new Vector3(rect.center.x, rect.yMin - _grid.cellSize.y);
            
            foreach (var handViewTileView in _handView.TileViews)
            {
                if(handViewTileView == null)
                    continue;
                
                handViewTileView.GetComponent<TileDrag>().OnTileMoved += OnTileMoved;
            }
        }


        private void CreateGrid()
        {
            _gridCells = new TileView[Model.Size.x, Model.Size.y];
            for (var x = 0; x < Model.Size.x; x++)
            {
                for (var y = 0; y < Model.Size.y; y++)
                {
                    var tile = Instantiate(_tileViewPrefab, _gridContainer);
                    tile.TileType = TileType.None;
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
                    if (Model.Tiles[x, y] == TileType.None)
                    {
                        continue;
                    }

                    var tile = Instantiate(_tileViewPrefab, _cellsContainer);
                    tile.TileType = Model.Tiles[x, y];
                    tile.transform.position = _grid.GetCellCenterWorld(new Vector3Int(x, y));
                    _tileCells[x, y] = tile;
                }
            }
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