using UnityEngine;

namespace SurfaceSystem.Data
{
    public enum SurfaceType
    {
        Wall,
        Floor, 
        Ceiling
    }

    [System.Flags]
    public enum MaterialType
    {
        None = 0,
        Base = 1 << 0,           // Основа
        Primer = 1 << 1,         // Грунтовка
        Putty = 1 << 2,          // Шпаклёвка
        Leveling = 1 << 3,       // Выравнивание
        Plaster = 1 << 4,        // Штукатурка
        SelfLeveling = 1 << 5,   // Наливной пол
        Underlayment = 1 << 6,   // Подложка
        Paint = 1 << 7,          // Краска
        Coating = 1 << 8,        // Лак
    }

    [CreateAssetMenu(fileName = "New Material", menuName = "SurfaceSystem/Material Definition")]
    public class MaterialDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;

        [Header("Тип этого материала")]
        [SerializeField] private MaterialType _materialType;
        [Header("Типы материалов на которые можно поставить слой")]
        [SerializeField] private MaterialType _requiredBaseMaterialsType;

        [Header("Завершающий ли слой")]
        [SerializeField] private bool _isFinalLayer = false;

        [Space(10)]
        [SerializeField] private float _dryingTimeSeconds = 5f;
        [SerializeField] private float _thicknessMeters = 0.005f;

        [Space(10), Header("Повторное нанесение")]
        [SerializeField] private SurfaceTypeDefinition _allowedSurfaceType;

        [Space(10)]
        [SerializeField] private float _adhesionToPreviousLayer = 1.0f;
        [SerializeField] private float _minAdhesionRequired = 0.5f;

        [SerializeField] private float _adhesionLossPerRepeatLayer = 0.1f;

        [Space(10)]
        [SerializeField] private bool _canBeAppliedMultipleTimes = true;

        public string DisplayName => _displayName;

        public MaterialType MaterialType => _materialType;
        public MaterialType RequiredBaseMaterialsType => _requiredBaseMaterialsType;

        public bool IsFinalLayer => _isFinalLayer;

        public float DryingTimeSeconds => _dryingTimeSeconds;
        public float ThicknessMeters => _thicknessMeters;

        public SurfaceTypeDefinition AllowedSurfaceType => _allowedSurfaceType;


        public float AdhesionToBaseOrPreviousLayer  => _adhesionToPreviousLayer;
        public float MinAdhesionRequired => _minAdhesionRequired;

        public float AdhesionLossPerRepeatLayer => _adhesionLossPerRepeatLayer;

        public bool CanBeAppliedMultipleTimes => _canBeAppliedMultipleTimes;

        public bool IsSameType(MaterialDefinition other) => this == other;
        public bool CanApplyToSurface(SurfaceTypeDefinition surfaceType) => _allowedSurfaceType == null || _allowedSurfaceType == surfaceType;
    }
}