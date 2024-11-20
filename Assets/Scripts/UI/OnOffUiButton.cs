using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class OnOffUiButton : MonoBehaviour
    {
        [SerializeField]
        private Sprite _onSprite;
        
        [SerializeField]
        private Sprite _offSprite;
        
        [SerializeField]
        private Image _image;
        
        private bool _state;
        private Func<bool> _getState;
        private Action<bool> _onClick;

        public void Setup(Action<bool> onClick, Func<bool> getState)
        {
            _onClick = onClick;
            _getState = getState;
            UpdateState();
        }

        private void UpdateState()
        {
            _state = _getState.Invoke();
            _image.sprite = _state ? _onSprite : _offSprite;
        }

        public void OnClick()
        {
            _onClick.Invoke(_state);
            UpdateState();
        }
    }
}