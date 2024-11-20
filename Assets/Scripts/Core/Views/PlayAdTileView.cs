using System;
using DG.Tweening;
using Modules.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Views
{
    public class PlayAdTileView : MonoBehaviour, IPointerClickHandler, IPooledGameObject
    {
        [SerializeField]
        private Animator _animator;

        private static readonly int Shake = Animator.StringToHash("shake");

        public event Action OnClick;
        
        public void OnPointerClick(PointerEventData _) => OnClick?.Invoke();

        public void OnEnable() => InvokeRepeating(nameof(Pulse), 0, 8);

        public void OnDisable() => CancelInvoke(nameof(Pulse));

        private void Pulse() => _animator.SetTrigger(Shake);

        string IPooledGameObject.Id => nameof(PlayAdTileView);
        GameObject IPooledGameObject.GameObject => gameObject;
        void IPooledGameObject.Reset()
        {
            _animator.Rebind();
            _animator.Update(0);
            OnClick = null;
        }
    }
}