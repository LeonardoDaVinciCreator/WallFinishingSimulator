namespace SurfaceSystem.UI.MaterialFilter
{
    public class MaterialFilterModel
    {
        private bool _showAllMaterials = false;

        public bool ShowAllMaterials => _showAllMaterials;

        public void SetShowAllMaterials(bool isShow)
        {
            if (_showAllMaterials == isShow) return;
    
            _showAllMaterials = isShow;
        }

        public void Toggle()
        {
            SetShowAllMaterials(!_showAllMaterials);
        }
    }
}