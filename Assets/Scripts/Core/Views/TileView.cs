using System;
using Core.Models;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Core.Board.Views;
using Modules.ComponentWithModel;
using Modules.DiContainer;
using Modules.Extensions;
using Modules.SoundService;
using Modules.Utils;
using Services.GamePlayerDataService;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Views
{
    public class TileView : ComponentWithModel<TileModel>, IBeginDragHandler, IDragHandler, IEndDragHandler, IPooledGameObject
    {
        [SerializeField]
        private SpriteRenderer _spriteRenderer;
        
        [SerializeField]
        private Animator _animator;
        
        [SerializeField]
        private AnimatorWaiter _animatorWaiter;
        
        [SerializeField]
        private TileType _type;
        
        [SerializeField]
        private TileOrderSorter _orderSorter;
        
        [SerializeField]
        private float _verticalOffset;

        private ISoundService SoundService { get; set; }
        private GamePlayerDataService PlayerData { get; set; }
        private BoardView _boardView;
        private Vector3 _positionBeforeDrag;
        private static readonly int DragTrigger = Animator.StringToHash("Drag");
        private static readonly int PlaceTrigger = Animator.StringToHash("Place");
        private static readonly int RevertTrigger = Animator.StringToHash("Revert");
        private static readonly int IdleTrigger = Animator.StringToHash("Idle");
        private static readonly int AppearTrigger = Animator.StringToHash("Appear");
        private static readonly int AppearOnHandTrigger = Animator.StringToHash("AppearOnHand");
        private static readonly int DisappearTrigger = Animator.StringToHash("Disappear");
        private static readonly string IdleState = "Idle";
        private static readonly string InactiveState = "Inactive";

        public bool IsDraggable { get; set; }
        public TileType Type => _type;

        protected override void OnInitialize(params object[] parameters)
        {
            Debug.Assert(parameters.Length == 1 && parameters[0] is BoardView);
            Debug.Assert(Model.Type == _type);
            _boardView = (BoardView)parameters[0];
            _orderSorter.SetOrder((int)Type);
            SoundService = Container.Resolve<ISoundService>();
            PlayerData = Container.Resolve<GamePlayerDataService>();
        }
        
        #region Drag
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!IsDraggable)
            {
                return;
            }
            
            _positionBeforeDrag = transform.position;
            _animator.SetTrigger(DragTrigger);
            _orderSorter.Foreground = true;
        }
        
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!IsDraggable)
            {
                return;
            }
            
            transform.position = Camera.main.ScreenToWorldPoint(eventData.position);
            transform.position = new Vector3(transform.position.x, transform.position.y + _verticalOffset, 0);
            _boardView.OnTileDrag(this);
        }
        
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!IsDraggable)
            {
                return;
            }
            
            _orderSorter.Foreground = false;
            _boardView.OnTileDragEnd(this);
        }
        #endregion
        
        public async UniTask MoveTo(Vector3 position)
        {
            _orderSorter.Foreground = false;
            await transform.DOMove(position, 0.2f).SetEase(Ease.OutQuad);
        }
        
        public async UniTask Disappear()
        {
           _animator.SetTrigger(DisappearTrigger);
           await _animatorWaiter.WaitState(InactiveState, 5);
        }

        public async UniTask Appear()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.1));
            _animator.SetTrigger(AppearTrigger);
            if (PlayerData.PlayerData.SoundEnabled)
            {
                SoundService.Play("pop");
            }
            await _animatorWaiter.WaitState(IdleState, 5);
        }
        
        public async UniTask AppearOnHand()
        {
            _animator.SetTrigger(AppearOnHandTrigger);
            await _animatorWaiter.WaitState(IdleState, 5);
        }
        
        public async UniTask Idle()
        {
            _animator.SetTrigger(IdleTrigger);
            await _animatorWaiter.WaitState(IdleState, 5);
        }
        
        public void RestoreHandPosition()
        {
            transform.DOMove(_positionBeforeDrag, 0.2f).SetEase(Ease.OutBack);
            _positionBeforeDrag = Vector3.zero;
            _animator.SetTrigger(RevertTrigger);
        }
        
        public async UniTask PlaceOnBoard()
        {
            _animator.SetTrigger(PlaceTrigger);
            if (PlayerData.PlayerData.SoundEnabled)
            {
                SoundService.Play("pop");
            }
            await _animatorWaiter.WaitState(IdleState, 5);
        }
        
        string IPooledGameObject.Id => _type.ToString();

        GameObject IPooledGameObject.GameObject => gameObject;

        void IPooledGameObject.Reset()
        {
            Release();
            transform.position = Vector3.zero;
            transform.localScale = Vector3.one;
            _animator.Rebind();
            _animator.Update(0f);
            _orderSorter.Foreground = false;
            _orderSorter.RestoreOrder();
        }

        private void OnValidate()
        {
            _verticalOffset = _spriteRenderer.bounds.size.y;
        }
    }
}