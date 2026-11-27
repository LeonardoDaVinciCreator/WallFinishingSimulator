using UnityEngine;
using SurfaceSystem.Events;
using SurfaceSystem.Components;

namespace SurfaceSystem.UI.DryingTimer
{
    public class DryingTimerController : MonoBehaviour
    {
        [SerializeField] private DryingTimerView _view;
        [SerializeField] private BaseSurfaceController _targetSurface;

        private DryingTimerModel _model;
        private bool _isGlobalMode;

        private void Start()
        {
            _model = new DryingTimerModel();

            if (_targetSurface == null)
            {
                _isGlobalMode = true;
                SurfaceEvents.OnSurfaceSelected += OnSurfaceSelected;
            }
            else
            {
                _model.SetSurface(_targetSurface);
            }
        }

        private void OnDestroy()
        {
            if (_isGlobalMode)
                SurfaceEvents.OnSurfaceSelected -= OnSurfaceSelected;
        }

        private void OnSurfaceSelected(BaseSurfaceController surface)
        {
            _model.SetSurface(surface);
        }

        private void Update()
        {
            if (_model.HasSurface)
            {
                _view.UpdateTimer(_model.RemainingTime);
                _view.UpdateAdhesion(_model.AdhesionValue, _model.IsAdhesionSufficient());
            }
            else
            {
                _view.ShowIdle();
            }
        }
    }
}