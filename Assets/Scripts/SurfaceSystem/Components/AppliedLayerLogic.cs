using SurfaceSystem.Core;
using SurfaceSystem.Data;
using UnityEngine;

namespace SurfaceSystem.Components
{
    public class AppliedLayerLogic : MonoBehaviour
    {
        [SerializeField] private MaterialDefinition _material;
        [SerializeField] private float _appliedTime;
        [SerializeField] private bool _isInitialized;
        [Header("настройка адгезии и толщины")]
        [SerializeField] private float _baseAdhesion = 1.0f;
        [SerializeField] private float _additionalThickness = 0.0f;

        public MaterialDefinition Material => _material;
        public float BaseAdhesion => _baseAdhesion;
        public float CurrentThicknessMultiplier => _additionalThickness;

        public void Initialize(MaterialDefinition material)
        {
            _material = material;
            _appliedTime = Time.time;
            _isInitialized = true;
            _baseAdhesion = material.AdhesionToBaseOrPreviousLayer;
            _additionalThickness = 0.0f;            

            gameObject.SetActive(true);
        }
        
        public float GetEffectiveAdhesionForNextLayer()
        {
            if (!_isInitialized || _material == null) return 0f;
            if (IsDried()) return _baseAdhesion;
            float progress = 1f - (GetRemainingDryingTime() / _material.DryingTimeSeconds);
            return _baseAdhesion * progress;
        }

        public void UpdateAdhesion(float newBaseAdhesion)
        {
            _baseAdhesion = Mathf.Clamp(newBaseAdhesion, 0f, 1f);
        }

        public void AddAdditionalThickness()
        {
            _additionalThickness += _material.ThicknessMeters;            
        }

        public bool IsDried()
        {
            if (!_isInitialized) return false;
            return (Time.time - _appliedTime) >= _material.DryingTimeSeconds;
        }

        public float GetRemainingDryingTime()
        {
            if (!_isInitialized || _material == null) return 0f;
            float elapsed = Time.time - _appliedTime;
            float remaining = _material.DryingTimeSeconds - elapsed;
            return Mathf.Max(0f, remaining);
        }

        public void ShowLayer() => gameObject.SetActive(true);
        public void HideLayer() => gameObject.SetActive(false);

        public float GetThickness()
        {
            if (_material == null) return 0f;
            return _material.ThicknessMeters + _additionalThickness;
        }

        public float GetLastAddedThickness()
        {
            return _material != null
                ? _material.ThicknessMeters
                : 0f;
        }

        public bool IsAdhesionSufficient()
        {
            if (_material == null) return false;
            return GetEffectiveAdhesionForNextLayer() >= _material.MinAdhesionRequired;
        }
    }
}