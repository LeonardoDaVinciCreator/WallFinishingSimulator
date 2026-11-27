using UnityEngine;

namespace SurfaceSystem.UI.SurfaceHeight
{
    public class SurfaceHeightController : MonoBehaviour
    {
        [SerializeField] private SurfaceHeightView _view;

        private SurfaceHeightService _service;
        private SurfaceHeightModel _model;        
        private float _heightOffset = 0f;

        private void Start()
        {
            
            _service = new SurfaceHeightService();
            _model = new SurfaceHeightModel(_service);
            _view.UpdateToggleUI(_model.IsTopSelected);
            _view.ResetSlider();
        }

        public void OnToggleClicked()
        {
            _model.IsTopSelected = !_model.IsTopSelected;
            _view.UpdateToggleUI(_model.IsTopSelected);
            _view.ResetSlider();
            _heightOffset = 0f;
        }

        public void OnSliderChanged(float value)
        {
            _heightOffset = value;
        }

        public void OnApplyClicked()
        {
            _model.ApplyOffset(_heightOffset);
            _view.ResetSlider();
            _heightOffset = 0f;
            _model.UpdateAllSurfaces();
        }

        public void OnCancelClicked()
        {
            _model.Cancel();
            _view.ResetSlider();
            _heightOffset = 0f;
            _model.UpdateAllSurfaces();
        }
    }
}