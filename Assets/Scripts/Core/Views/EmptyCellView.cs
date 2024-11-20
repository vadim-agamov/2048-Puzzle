using Modules.Utils;
using UnityEngine;

namespace Core.Views
{
    public class EmptyCellView : MonoBehaviour, IPooledGameObject
    {
        [SerializeField]
        private Animator _animator;
        
        private int HOVERED = Animator.StringToHash("hovered");
        
        public void HoverOut() => _animator.SetBool(HOVERED, false);
        public void HoverIn() => _animator.SetBool(HOVERED, true);

        string IPooledGameObject.Id => nameof(EmptyCellView);

        GameObject IPooledGameObject.GameObject => gameObject;

        void IPooledGameObject.Reset()
        {
            _animator.Rebind();
            _animator.Update(0);
        }
    }
}