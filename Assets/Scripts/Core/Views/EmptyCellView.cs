using UnityEngine;

namespace Core.Views
{
    public class EmptyCellView : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;
        
        private int HOVERED = Animator.StringToHash("hovered");

        public void HoverOut() => _animator.SetBool(HOVERED, false);
        public void HoverIn() => _animator.SetBool(HOVERED, true);
    }
}