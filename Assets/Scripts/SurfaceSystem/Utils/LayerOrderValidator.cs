using SurfaceSystem.Data;
using SurfaceSystem.Components;

namespace SurfaceSystem.Utils
{
    public static class LayerOrderValidator
    {
        public static bool CanApplySameLayerType(MaterialDefinition material, AppliedLayerLogic[] layers, int currentLayerCount)
        {
            if (currentLayerCount <= 0) return false;
            AppliedLayerLogic lastLayer = layers[currentLayerCount - 1];
            return lastLayer != null && lastLayer.Material != null && lastLayer.Material == material;
        }

        public static bool IsValidOrder(MaterialDefinition material, AppliedLayerLogic[] layers, int currentLayerCount)
        { 
            if(currentLayerCount == 0)
            {
                return material.MaterialType == MaterialType.Base;              
            }
                 
            MaterialDefinition previousMaterial = layers[currentLayerCount - 1].Material;

            if (material.RequiredBaseMaterialsType != MaterialType.None)
            {
                return (previousMaterial.MaterialType & material.RequiredBaseMaterialsType) != 0;
            }

            return false;
        }

        public static string GetOrderErrorMessage(MaterialDefinition material, AppliedLayerLogic[] layers, int currentLayerCount)
        {
            if (currentLayerCount == 0)
            {
                return $"Слой {material.DisplayName} должен быть нанесён на основание";
            }

            MaterialDefinition previousMaterial = layers[currentLayerCount - 1].Material;
            string requiredTypes = material.RequiredBaseMaterialsType.ToString();
            string currentName = previousMaterial.DisplayName;
            
            return $"Слой {material.DisplayName} [тип {material.MaterialType}] нельзя нанести на {previousMaterial.DisplayName} [тип {previousMaterial.MaterialType}]. Требуется один из типов: {requiredTypes}";
        }

        public static string GetApplySuccessMessage(MaterialDefinition material, AppliedLayerLogic[] layers, int currentLayerCount)
        {            
            string previousName = layers[currentLayerCount - 1].Material.DisplayName;
            return $"{material.DisplayName} нанесён на {previousName}";
        }

        public static string GetThickenSuccessMessage(MaterialDefinition material)
        {
            return $"Слой {material.DisplayName} утолщён";
        }

        public static string GetResetMessage(BaseSurfaceController surface)
        {
            return $"Слои на поверхности {surface.SurfaceTypeDefinition.DisplayName} сброшены";
        }        
    }
}