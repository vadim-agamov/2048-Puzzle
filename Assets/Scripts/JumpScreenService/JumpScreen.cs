using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Services.JumpScreenService
{
    public class JumpScreen : MonoBehaviour, IJumpScreenService
    {
        [SerializeField]
        private CanvasGroup _canvasGroup;
        
        [SerializeField] 
        private TMP_Text _progressText;
        
        [SerializeField] 
        private TMP_Text _appVersionText;
        
        
        async UniTask IJumpScreenService.Show(CancellationToken cancellationToken)
        {
            gameObject.SetActive(true);
            _progressText.gameObject.SetActive(false);
            _canvasGroup.alpha = 0;
            await _canvasGroup
                .DOFade(1, 0.5f)
                .SetEase(Ease.InQuad)
                .ToUniTask(cancellationToken: cancellationToken);
        }

        async UniTask IJumpScreenService.Hide(CancellationToken cancellationToken)
        {
            await _canvasGroup
                .DOFade(0, 0.5f)
                .SetEase(Ease.InQuad)
                .ToUniTask(cancellationToken: cancellationToken);
            gameObject.SetActive(false);
        }
        
        public void Start()
        {
            _progressText.gameObject.SetActive(false);
            _appVersionText.text = $"v{Application.version}";
            DontDestroyOnLoad(gameObject);
        }

        public void Report(float value)
        {
            Debug.Log($"[{nameof(JumpScreen)}] progress: {value}");
            _progressText.gameObject.SetActive(true);
            _progressText.text = $"{(100 * value):0}%";
        }
    }
}
