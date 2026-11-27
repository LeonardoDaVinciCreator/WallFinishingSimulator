using System;
using SurfaceSystem.Data;
using SurfaceSystem.Components;
using UnityEngine;

namespace SurfaceSystem.Events
{
    public static class SurfaceEvents
    {
        public static event Action<BaseSurfaceController> OnSurfaceSelected;

        public static event Action<BaseSurfaceController, MaterialDefinition> OnLayerApplied;

        public static event Action<BaseSurfaceController, string> OnLayerError;

        public static event Action<Transform> OnCornerMoved;

        public static event Action<BaseSurfaceController, float> OnSurfaceThicknessChanged;

        public static event Action<BaseSurfaceController> OnSurfaceRegistered;

        public static event Action<BaseSurfaceController> OnSurfaceReset;

        public static event Action<BaseSurfaceController> OnSurfaceCompleted;

        public static event Action<bool> OnMaterialFilterChanged;

        public static event Action<string> OnNotification;

        public static void RaiseWallSelected(BaseSurfaceController wall) => OnSurfaceSelected?.Invoke(wall);

        public static void RaiseLayerApplied(BaseSurfaceController wall, MaterialDefinition mat) => OnLayerApplied?.Invoke(wall, mat);
       
        public static void RaiseLayerError(BaseSurfaceController wall, string error) => OnLayerError?.Invoke(wall, error);

        public static void RaiseCornerMoved(Transform corner) => OnCornerMoved?.Invoke(corner);

        public static void RaiseSurfaceThicknessChanged(BaseSurfaceController surface, float thickness) => OnSurfaceThicknessChanged?.Invoke(surface, thickness);

        public static void RaiseSurfaceRegistered(BaseSurfaceController surface) => OnSurfaceRegistered?.Invoke(surface);

        public static void RaiseSurfaceReset(BaseSurfaceController surface) => OnSurfaceReset?.Invoke(surface);

        public static void RaiseSurfaceCompleted(BaseSurfaceController surface) => OnSurfaceCompleted?.Invoke(surface);
        
        public static void RaiseMaterialFilterChanged(bool showAllMaterials) => OnMaterialFilterChanged?.Invoke(showAllMaterials);

        public static void RaiseNotification(string message) => OnNotification?.Invoke(message);

    }
}