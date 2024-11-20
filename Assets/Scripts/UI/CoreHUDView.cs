using DG.Tweening;
using Modules.UIService;
using TMPro;
using UnityEngine;

namespace UI
{
    public class CoreHUDView : UIView<CoreHUDModel>
    {
        [SerializeField]
        private TMP_Text _scoreText;
        
        [SerializeField]
        private TMP_Text _topScoreText;
        
        [SerializeField]
        private OnOffUiButton _soundButton;
        
        [SerializeField]
        private OnOffUiButton _musicButton;
        
        private float _visualScore;
        private int _score;
        private Sequence _sequence;
        
        protected override void OnSetModel()
        {
            _score = Model.Score;
            _scoreText.text = _score.ToString();
            _topScoreText.text = Model.TopScore.ToString();
            
            _soundButton.Setup(Model.SetSoundEnabled, () => Model.Sound);
            _musicButton.Setup(Model.SetMusicEnabled, () => Model.Music);
        }
        
        public void UpdateScore(string _, int v)
        {
            _score += v;
            _scoreText.text = _score.ToString();
            
            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            _sequence
                .Append(_scoreText.transform.DOScale(1.2f, 0.1f))
                .Append(_scoreText.transform.DOScale(1f, 0.1f))
                .Play();
        }
    }
}