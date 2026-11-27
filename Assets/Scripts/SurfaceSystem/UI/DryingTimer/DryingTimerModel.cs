using SurfaceSystem.Data;
using SurfaceSystem.Components;

namespace SurfaceSystem.UI.DryingTimer
{
    public class DryingTimerModel
    {
        private BaseSurfaceController _surface;
        private float _minRequiredAdhesion = 0.5f;

        public bool HasSurface => _surface != null && _surface.CurrentLayerCount > 0;
        public float RemainingTime => _surface?.GetLastLayerRemainingTime() ?? 0f;
        public float AdhesionValue => _surface?.GetLastLayerAdhesion() ?? 0f;
        public MaterialDefinition LastMaterial => _surface?.GetLastLayerMaterial();

        public void SetSurface(BaseSurfaceController surface)
        {
            _surface = surface;
            if (_surface != null && _surface.CurrentLayerCount > 0)
            {
                var mat = _surface.GetLastLayerMaterial();
                if (mat != null) _minRequiredAdhesion = mat.MinAdhesionRequired;
            }
        }

        public bool IsAdhesionSufficient() => AdhesionValue >= _minRequiredAdhesion;
    }
}