using SurfaceSystem.Components;
using SurfaceSystem.Data;

namespace SurfaceSystem.UI.SurfaceButton
{
    public class SurfaceButtonModel
    {
        private BaseSurfaceController _surface;
        private int _surfaceNumber = 1;

        public BaseSurfaceController Surface => _surface;
        public string SurfaceName => GetSurfaceTypeName() + " " + _surfaceNumber;

        public void SetSurface(BaseSurfaceController surface, int number)
        {
            _surface = surface;
            _surfaceNumber = number;
        }

        private string GetSurfaceTypeName()
        {
            switch (_surface.SurfaceType)
            {
                case SurfaceType.Wall: return "Стена";
                case SurfaceType.Floor: return "Пол";
                case SurfaceType.Ceiling: return "Потолок";
                default: return "Поверхность";
            }
        }

        public void SelectSurface()
        {
            if (_surface != null && RepairUIManager.Instance != null)
            {
                RepairUIManager.Instance.SelectWall(_surface);
                
                SurfaceType type = GetSurfaceType();
                RepairUIManager.Instance.ShowMaterialPanel(type);
            }
        }

        private SurfaceType GetSurfaceType()
        {
            return _surface.SurfaceType;
        }
    }
}