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
        
        private float _visualScore;
        private int _score;
        
        protected override void OnSetModel()
        {
            _score = Model.Score;
            _scoreText.text = _score.ToString();
            _topScoreText.text = Model.TopScore.ToString();
        }
        
        public void UpdateScore(string _, int v)
        {
            _score += v;
        }

        private void Update()
        {
            if((int)_visualScore != _score)
            {
                _visualScore = Mathf.MoveTowards(_visualScore, _score, Time.deltaTime * 50);
                _scoreText.text = ((int)_visualScore).ToString();
            }
        }
    }
}