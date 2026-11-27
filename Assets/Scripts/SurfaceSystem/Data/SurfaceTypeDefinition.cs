using UnityEngine;

namespace SurfaceSystem.Data
{
    [CreateAssetMenu(fileName = "SurfaceType", menuName = "SurfaceSystem/Surface Type Definition")]
    public class SurfaceTypeDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName = "Поверхность";
        [SerializeField] private SurfaceType _surfaceType;
        
        public string DisplayName => _displayName;
        public SurfaceType SurfaceType => _surfaceType;
    }
}