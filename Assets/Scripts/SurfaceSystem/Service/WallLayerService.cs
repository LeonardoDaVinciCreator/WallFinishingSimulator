using SurfaceSystem.Data; 
using SurfaceSystem.Utils;
using SurfaceSystem.Components;
using SurfaceSystem.Core;
using SurfaceSystem.Events;
using UnityEngine;

namespace SurfaceSystem.Service
{
    public class WallLayerService
    {
        private readonly AppliedLayerLogic[] _layers;
        private readonly BaseSurfaceController _surface;

        private int _index = 0;

        private bool _hasInitialLayer;

        public int CurrentLayerCount => _index;

        private const float BaseWallThickness = 0.0f;

        public WallLayerService(AppliedLayerLogic[] layers, BaseSurfaceController wall)
        {
            _layers = layers;
            _surface = wall;
        }

        public void Reset()
        {
            int startIndex =
            _hasInitialLayer ? 1 : 0;

            for (int i = startIndex; i < _layers.Length; i++)
            {
                if (_layers[i] != null)
                {
                    _layers[i].gameObject.SetActive(false);
                }
            }

            _index = _hasInitialLayer ? 1 : 0;

            NotifyThicknessChanged();
        }

        public bool TryApply(MaterialDefinition material, System.Action rebuildGeometry)
        {
            if (material == null || _index >= _layers.Length) return false;

            if (_index > 0 && _layers[_index - 1].Material.IsFinalLayer)
            {
                if (_layers[_index - 1].Material == material)
                {
                    return ApplySame(material, rebuildGeometry);
                }
                else
                {
                    SurfaceEvents.RaiseLayerError(_surface, $"Слой {_layers[_index - 1].Material.DisplayName} завершающий, нельзя наносить новые слои!");
                    return false;
                }
            }

            bool sameType = LayerOrderValidator.CanApplySameLayerType(material, _layers, _index);

            return sameType ? ApplySame(material, rebuildGeometry) : ApplyNew(material, rebuildGeometry);
        }

        private bool ApplySame(MaterialDefinition material, System.Action rebuild)
        {
            var layer = _layers[_index - 1];
            if (layer == null) return false;

            if (!material.CanBeAppliedMultipleTimes)
            {
                SurfaceEvents.RaiseLayerError(_surface, $"Слой {material.DisplayName} нельзя использовать повторно!");
                return false;
            }

            layer.AddAdditionalThickness();

            NotifyThicknessChanged();

            float adhesion = WallAdhesionCalculator.CalculateAdhesion(material, sameTypeLayer: layer);

            layer.UpdateAdhesion(adhesion);

            //rebuild();

            if (layer.GetEffectiveAdhesionForNextLayer() < material.MinAdhesionRequired)
            {
                layer.HideLayer();
                _index--;
                SurfaceEvents.RaiseLayerError(_surface, $"Недостаточная адгезия для слоя {material.DisplayName}!");
                return false;
            }
            
            NotifyThicknessChanged();

            SurfaceEvents.RaiseLayerApplied(_surface, material);
            
            return true;
        }

        private bool ApplyNew(MaterialDefinition material, System.Action rebuild)
        {
            if (material.AllowedSurfaceType != null && material.AllowedSurfaceType != _surface.SurfaceTypeDefinition)
            {
                SurfaceEvents.RaiseLayerError(_surface, $"Материал {material.DisplayName} нельзя наносить на поверхность {_surface.SurfaceTypeDefinition.DisplayName}");
                return false;
            }

            int layerIndex = -1;
            for (int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i] != null && _layers[i].Material == material)
                {
                    layerIndex = i;
                    break;
                }
            }
            if (layerIndex == -1)
            {
                SurfaceEvents.RaiseLayerError(_surface, $"Слой с материалом {material.DisplayName} не найден!");
                return false;
            }

            if (layerIndex > 0)
            {                
                MaterialDefinition previousMaterial = _index > 0 ? _layers[_index - 1].Material : null;
                if (previousMaterial != null)
                {
                    if (material.RequiredBaseMaterialsType != MaterialType.None)
                    {
                        if ((previousMaterial.MaterialType & material.RequiredBaseMaterialsType) == 0)
                        {
                            SurfaceEvents.RaiseLayerError(_surface, LayerOrderValidator.GetOrderErrorMessage(material, _layers, _index));
                            return false;
                        }
                    }
                }
                else
                {
                    if (material.MaterialType != MaterialType.Base)
                    {
                        SurfaceEvents.RaiseLayerError(_surface, "Первый слой должен быть базовым!");
                        return false;
                    }
                }
            }

            var layer = _layers[layerIndex];
            if (layer == null) return false;

            layer.Initialize(material);

            AppliedLayerLogic previousLayer = _index > 0 ? _layers[_index - 1] : null;

            float adhesion = WallAdhesionCalculator.CalculateAdhesion(material, previousLayer);
            layer.UpdateAdhesion(adhesion);

            if (!WallAdhesionCalculator.IsAdhesionSufficient(adhesion, material))
            {
                layer.HideLayer();
                SurfaceEvents.RaiseLayerError(_surface, $"Недостаточная адгезия для слоя {material.DisplayName}");
                return false;
            }

            _index = layerIndex + 1;

            if (material.IsFinalLayer)
            {
                SurfaceEvents.RaiseSurfaceCompleted(_surface);
            }

            NotifyThicknessChanged();
            SurfaceEvents.RaiseLayerApplied(_surface, material);
            return true;
        }

        public void ApplyInitialLayer(
            MaterialDefinition material)
        {
            if (material == null || _layers.Length == 0) return;

            var layer = _layers[0];
            if (layer == null) return;

            layer.Initialize(material);

            _index = 1;

            _hasInitialLayer = true;

            NotifyThicknessChanged();
        }

        public float GetRemainingTime() => _index == 0 ? 0 : _layers[_index - 1].GetRemainingDryingTime();

        public float GetAdhesion() => _index == 0 ? 0 : _layers[_index - 1].GetEffectiveAdhesionForNextLayer();

        public MaterialDefinition GetLastMaterial() => _index == 0 ? null : _layers[_index - 1].Material;

        private void NotifyThicknessChanged()
        {
            if (_surface == null) return;

            float totalThickness = GetTotalThickness();
            SurfaceEvents.RaiseSurfaceThicknessChanged(_surface, totalThickness);
        }

        public float GetTotalThickness()
        {
            float total = BaseWallThickness;

            for (int i = 0; i < _index; i++)
            {
                if (_layers[i] == null)
                    continue;

                total += _layers[i].GetThickness();
            }

            return total;
        }

        public void NotifyThicknessRebuild()
        {
            NotifyThicknessChanged();
        }
    }
}