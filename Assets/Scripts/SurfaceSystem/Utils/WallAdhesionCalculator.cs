using UnityEngine;

using SurfaceSystem.Data;
using SurfaceSystem.Components;


namespace SurfaceSystem.Utils
{
    public static class WallAdhesionCalculator
    {
        /// <summary>
        /// возвращает адгезию после высыхания
        /// </summary>        
        public static float CalculateAdhesion(
            MaterialDefinition material,
            AppliedLayerLogic previousLayer = null,
            AppliedLayerLogic sameTypeLayer = null)
        {
            // повторное нанесение материала
            if (sameTypeLayer != null)
            {
                return Mathf.Max(0f, sameTypeLayer.BaseAdhesion - material.AdhesionLossPerRepeatLayer);
            }
            // новый слой
            if (previousLayer != null) 
            {
                // эффективная адгезия с учетом высыхания
                return previousLayer.GetEffectiveAdhesionForNextLayer();
            }

            // Нанесение на голую стену
            return material.AdhesionToBaseOrPreviousLayer;
        }

        public static bool IsAdhesionSufficient(float calculatedAdhesion, MaterialDefinition material)
        {        
            return calculatedAdhesion >= material.MinAdhesionRequired;
        }
    }
}