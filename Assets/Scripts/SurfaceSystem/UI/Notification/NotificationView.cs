using UnityEngine;
using TMPro;
using DG.Tweening;

namespace SurfaceSystem.UI.Notification
{
    public class NotificationView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private RectTransform _panelRect;
        [SerializeField] private GameObject _panel;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Settings")]
        [SerializeField] private float _displayDuration = 2f;
        [SerializeField] private float _animationDuration = 0.5f;
        [SerializeField] private float _springStrength = 1.5f;

        private Tween _currentTween;

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = _panel.GetComponent<CanvasGroup>();
            
            if (_canvasGroup == null)
                _canvasGroup = _panel.AddComponent<CanvasGroup>();
            
            _canvasGroup.alpha = 0f;
            _panelRect.localScale = Vector3.zero;
            _panel.SetActive(false);
        }

        public void Show()
        {
            Show("fdsfsdf", true);
        }

        public void Show(string message, bool isSuccess = true)
        {
            _currentTween?.Kill();
            
            _messageText.text = message;
            _messageText.color = isSuccess ? Color.green : Color.red;
            
            _panel.SetActive(true);

            //появление
            Sequence sequence = DOTween.Sequence();//последовательность анимаций
            sequence.SetUpdate(true);
            
            _panelRect.localScale = Vector3.zero;
            sequence.Append(_panelRect.DOScale(1f, _animationDuration).SetEase(Ease.OutElastic, _springStrength));
            sequence.Join(_canvasGroup.DOFade(1f, _animationDuration * 0.5f));            
           
            sequence.AppendInterval(_displayDuration); // Пауза
            
            // исчезновение
            //sequence.Append()//добавляет новвые анимации к последовательности
            sequence.Append(_canvasGroup.DOFade(0f, _animationDuration * 0.5f));
            sequence.Join(_panelRect.DOScale(0f, _animationDuration * 0.3f).SetEase(Ease.InBack));//одновременно с предыдущей
            
            sequence.OnComplete(() => 
            {
                _panel.SetActive(false);
                _canvasGroup.alpha = 0f;
            });
            
            _currentTween = sequence;
        }
    }
}