using Core.Configs;
using Core.Controller;
using Core.Models;
using Cysharp.Threading.Tasks;
using Modules.ComponentWithModel;
using Modules.Extensions;
using Modules.UiComponents;
using Modules.UiComponents.UiRectsManager;
using Modules.UIService;
using Modules.Utils;
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
        private EmptyCellView _emptyCellPrefab;

        [SerializeField] 
        private HandView _handView;
        
        [SerializeField]
        private TilesConfig _tilesConfig;
        
        [SerializeField]
        private SpriteRenderer _background;
        
        [SerializeField]
        private SpriteRenderer _outline;
        
        [SerializeField]
        private float _visibleRectOffset = 0.5f;
        
        private EmptyCellView[,] _gridCells;
        private TileView[,] _tileCells;
        private Vector2Int _hoveredCell = Vector2Int.zero;
        private GameObjectsPool _tilePool;
        private UiRect _boardUiRect;
        private IUIService _uiService;

        private BoardModel Model { get; set; }
        private BoardController Controller { get; set; }
        public TileView[,] TileCells => _tileCells;
        public HandView HandView => _handView;
        public GameObjectsPool TilePool => _tilePool;
        
        public void Initialize(BoardController controller, BoardModel model, UIService uiService)
        {
            _tilePool = new GameObjectsPool(new GameObject("[POOL]").transform);
            _uiService = uiService;
            Model = model;
            Controller = controller;
            CreateGrid();
            CreateCells();
            CreateHand();
            FitBoardOutline();
            FitCamera().Forget();
        }
        
        public void OnTileDrag(TileView tileView)
        {
            var cell = (Vector2Int)_grid.WorldToCell(tileView.transform.position);
            if (cell.x < 0 || cell.x > Model.Size.x - 1 ||
                cell.y < 0 || cell.y > Model.Size.y - 1 ||
                _tileCells[cell.x, cell.y] != null)
            {
                _gridCells.ForEach(v => v.HoverOut());
                return;
            }   
            
            if (_hoveredCell != cell) 
            {
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
                tileView.RestoreHandPosition();
                return;
            }  
            
            _gridCells[_hoveredCell.x, _hoveredCell.y].HoverOut();
            Controller.PutTileOnBoard(cell, tileView);
        }
        
        public void ReloadHand()
        {
            _handView.Release();
            _handView.Initialize(Model.Hand, this, Controller);
        }

        private void CreateHand()
        {
            _handView.Initialize(Model.Hand, this, Controller);
            var rect = VisibleGridWorldRect;
            _handView.transform.position = new Vector3(rect.center.x, rect.yMin - _grid.cellSize.y);
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
                    
                    CreateTile(model);
                }
            }
        }

        public TileView CreateTile(TileModel model)
        {
            var tile = model.CreateView(_tilePool.Get, _tilesConfig.GetPrefab(model.Type), _cellsContainer, this);
            tile.transform.position = _grid.GetCellCenterWorld(new Vector3Int(model.BoardPosition.x, model.BoardPosition.y));
            _tileCells[model.BoardPosition.x, model.BoardPosition.y] = tile;
            return tile;
        }

        public void ReleaseTile(TileView tileView) => _tilePool.Release(tileView);

        public TileView RemoveTile(Vector2Int position)
        {
            var tileView = _tileCells[position.x, position.y];
            _tileCells[position.x, position.y] = null;
            return tileView;
        }

        public async UniTask PutTileFromHand(Vector2Int position, TileView tileView)
        {
            Debug.Assert(_tileCells[position.x, position.y] == null, $"Tile already exists {position}");
            
            tileView.transform.position = _grid.GetCellCenterWorld(new Vector3Int(position.x, position.y));
            tileView.transform.SetParent(_cellsContainer);
            tileView.IsDraggable = false;
            _tileCells[position.x, position.y] = tileView;
            _handView.RemoveTile(tileView);

            await tileView.PlaceOnBoard();
        }

        private async UniTask FitCamera()
        {
            if (_boardUiRect == null)
            {
                UiRectsManager.TryGetWorldRect("board", out _boardUiRect);
                _boardUiRect.OnRectTransformDimensionsChanged += () => FitCamera().Forget();
            }

            await _camera.FitOrthographic(VisibleWorldRect, _boardUiRect.RectTransform);
            FitBackToCamera();
        }
        
        private void FitBackToCamera()
        {
            _background.size = _camera.orthographicSize * 2 * new Vector2(_camera.aspect, 1);
            _background.transform.position = new Vector3(_camera.transform.position.x, _camera.transform.position.y, 0);
        }
        
        private void FitBoardOutline()
        {
            _outline.size = VisibleWorldRect.size;
            _outline.transform.position = VisibleWorldRect.center;
        }

        private Rect VisibleWorldRect => VisibleGridWorldRect.Union(_handView.VisibleWorldRect).Expand(_visibleRectOffset);

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