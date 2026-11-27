using UnityEngine;
using TMPro;

namespace SurfaceSystem.UI.DryingTimer
{
    public class DryingTimerView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _adhesionText;

        [SerializeField] private string _dryMessage = "Готово";
        [SerializeField] private string _dryingFormat = "Высыхание: {0:F1} сек";
        [SerializeField] private string _idleMessage = "Ожидание слоёв";
        [SerializeField] private string _adhesionFormat = "Адгезия: {0:P1}";

        public void UpdateTimer(float remainingTime)
        {
            if (_timerText == null) return;

            if (remainingTime <= 0f)
            {
                _timerText.text = _dryMessage;
                _timerText.color = Color.green;
            }
            else
            {
                _timerText.text = string.Format(_dryingFormat, remainingTime);
                _timerText.color = Color.red;
            }
        }

        public void UpdateAdhesion(float adhesion, bool isSufficient)
        {
            if (_adhesionText == null) return;

            _adhesionText.text = string.Format(_adhesionFormat, adhesion);
            _adhesionText.color = isSufficient ? Color.green : Color.red;
        }

        public void ShowIdle()
        {
            if (_timerText != null)
            {
                _timerText.text = _idleMessage;
                _timerText.color = Color.white;
            }
            if (_adhesionText != null) _adhesionText.text = "";
        }
    }
}