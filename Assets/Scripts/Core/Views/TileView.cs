using System;
using System.Linq;
using Core.Models;
using Modules.Extensions;
using UnityEngine;

namespace Core.Views
{
    public class TileView : ComponentWithModel<TileType>
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
        
        private int HOVERED = Animator.StringToHash("hovered");

        public TileType TileType => Model;
        
        protected override void OnInitialize()
        {
            _spriteRenderer.sprite = _tileSprites.Single(x => x.Type == Model).Sprite;
        }
        
        public void HoverOut() => _animator.SetBool(HOVERED, false);
        public void HoverIn() => _animator.SetBool(HOVERED, true);
    }
}