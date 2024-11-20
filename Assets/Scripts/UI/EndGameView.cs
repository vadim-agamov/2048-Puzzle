using Cysharp.Threading.Tasks;
using Modules.UIService;
using TMPro;
using UnityEngine;

namespace UI
{
    public class EndGameModel : UIModel
    {
        public int Score { get; }
        public int TopScore { get; }
        
        public EndGameModel(int score, int topScore)
        {
            Score = score;
            TopScore = topScore;
        }
    }
    
    public class EndGameView : UIView<EndGameModel>
    {
        [SerializeField]
        private TMP_Text _scoreText;
        
        [SerializeField]
        private TMP_Text _topScoreText;
        
        protected override void OnSetModel()
        {
            _scoreText.text = Model.Score.ToString();
            _topScoreText.text = Model.TopScore.ToString();
        }

        public void Close() => Model.HideAndClose(Bootstrapper.SessionToken).Forget();
    }
}