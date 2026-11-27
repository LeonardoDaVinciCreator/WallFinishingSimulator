using UnityEngine;
using UnityEngine.UI;

namespace SurfaceSystem.UI.SurfaceHeight
{
    public class SurfaceHeightView : MonoBehaviour
    {
        [SerializeField] private Button _toggleButton;
        [SerializeField] private Image _topToggleIcon;
        [SerializeField] private Image _bottomToggleIcon;
        [SerializeField] private Slider _heightSlider;
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _cancelButton;

        public Button ToggleButton => _toggleButton;
        public Image TopToggleIcon => _topToggleIcon;
        public Image BottomToggleIcon => _bottomToggleIcon;
        public Slider HeightSlider => _heightSlider;
        public Button ApplyButton => _applyButton;
        public Button CancelButton => _cancelButton;

        public void UpdateToggleUI(bool isTopSelected)
        {
            TopToggleIcon.gameObject.SetActive(isTopSelected);
            BottomToggleIcon.gameObject.SetActive(!isTopSelected);
        }

        public void ResetSlider()
        {
            HeightSlider.value = 0f;
        }
    }
}