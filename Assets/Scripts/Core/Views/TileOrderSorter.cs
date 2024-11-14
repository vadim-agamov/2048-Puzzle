using UnityEngine;

namespace Game.Core.Board.Views
{
    public class TileOrderSorter : MonoBehaviour
    {
        [SerializeField]
        private Renderer[] _renderers;

        [SerializeField, HideInInspector]
        private int[] _renderersOrder;

        private static int _gameplayLayer;
        private static int _gameplayForegroundLayer;
        
        private void Awake()
        {
            _gameplayLayer = SortingLayer.NameToID("Gameplay");
            _gameplayForegroundLayer = SortingLayer.NameToID("GameplayForeground");
            Foreground = false;
        }

        public bool Foreground
        {
            set
            {
                if (value)
                {
                    foreach (var spriteRenderer in _renderers)
                    {
                        spriteRenderer.sortingLayerID = _gameplayForegroundLayer;
                    }
                }
                else
                {
                    foreach (var spriteRenderer in _renderers)
                    {
                        spriteRenderer.sortingLayerID = _gameplayLayer;
                    }
                }
            }
            
            get => _renderers[0].sortingLayerID == _gameplayForegroundLayer;
        }
        
        public void SetOrder(int order)
        {
            for (var i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].sortingOrder =_renderersOrder[i] + order;
            }
        }
        
        public void RestoreOrder()
        {
            for (var i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].sortingOrder = _renderersOrder[i];
            }
        }
        
        private void OnValidate()
        {
            _renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            _renderersOrder = new int[_renderers.Length];
            for (var i = 0; i < _renderers.Length; i++)
            {
                _renderersOrder[i] = _renderers[i].sortingOrder;
            }
        }
    }
}
