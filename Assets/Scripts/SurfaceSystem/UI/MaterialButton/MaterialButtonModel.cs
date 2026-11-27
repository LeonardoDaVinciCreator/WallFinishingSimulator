using SurfaceSystem.Data;

namespace SurfaceSystem.UI.MaterialButton
{
    public class MaterialButtonModel
    {
        private MaterialDefinition _material;

        public MaterialDefinition Material => _material;
        public string DisplayName => _material != null ? _material.DisplayName : "Unknown";

        public void SetMaterial(MaterialDefinition material)
        {
            _material = material;
        }

        public void ApplyMaterial()
        {
            if (_material != null && RepairUIManager.Instance != null)
                RepairUIManager.Instance.SetSelectedMaterial(_material);
        }
    }
}