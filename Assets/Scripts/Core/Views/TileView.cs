using System;
using System.Linq;
using Core.Models;
using Modules.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Views
{
    public class TileView : ComponentWithModel<TileModel>, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Serializable]
        private class TileSprite
        {
            public TileType Type;
            public Sprite Sprite;
        }
        
        [SerializeField]
        private TileSprite[] _tileSprites;
        
        [SerializeField]
        private SpriteRenderer _spriteRenderer;
        
        [SerializeField]
        private Animator _animator;
        
        [SerializeField]
        private Collider2D _collider;
        
        private BoardView _boardView;
        
        private Vector3 _positionBeforeDrag;

        public bool IsDraggable;

        protected override void OnInitialize(params object[] parameters)
        {
            Debug.Assert(parameters.Length == 1 && parameters[0] is BoardView);
            _boardView = (BoardView)parameters[0];
            _spriteRenderer.sprite = _tileSprites.Single(x => x.Type == Model.Type).Sprite;
        }
        
        public void RestorePosition()
        {
            transform.position = _positionBeforeDrag;
            _positionBeforeDrag = Vector3.zero;
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsDraggable)
            {
                return;
            }
            
            _positionBeforeDrag = transform.position;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!IsDraggable)
            {
                return;
            }
            
            transform.position = Camera.main.ScreenToWorldPoint(eventData.position);
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            
            _boardView.OnTileDrag(this);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsDraggable)
            {
                return;
            }
            
            Debug.Log("OnEndDrag");
            _boardView.OnTileDragEnd(this);
        }
        
        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}