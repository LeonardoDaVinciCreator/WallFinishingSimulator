using UnityEngine; 
using SurfaceSystem.Data;
using SurfaceSystem.Core;
using SurfaceSystem.Service;
using System.Collections.Generic;
using SurfaceSystem.Utils;
using SurfaceSystem.Events;

namespace SurfaceSystem.Components
{
    public abstract class BaseSurfaceController : MonoBehaviour
    {
        protected RoomManager _roomManager;

        [SerializeField] private SurfaceTypeDefinition _surfaceTypeDefinition;
        
        [Header("Corners")]
        [SerializeField] protected Transform _cornerBottomLeft;
        [SerializeField] protected Transform _cornerBottomRight;
        [SerializeField] protected Transform _cornerTopLeft;
        [SerializeField] protected Transform _cornerTopRight;

        [SerializeField] protected AppliedLayerLogic[] _allLayers;

        [SerializeField] private MaterialDefinition _initialMaterial;
        [SerializeField] private SurfaceType _surfaceType;

        [Header("Outline")]
        [SerializeField] private Color _outlineColor = Color.yellow;
        [SerializeField] private float _outlineWidth = 0.05f;

        private SurfaceOutline _outline;        

        public Transform CornerBottomLeft => _cornerBottomLeft;
        public Transform CornerBottomRight => _cornerBottomRight;
        public Transform CornerTopLeft => _cornerTopLeft;
        public Transform CornerTopRight => _cornerTopRight;

        public SurfaceType SurfaceType => _surfaceType;
        public SurfaceTypeDefinition SurfaceTypeDefinition => _surfaceTypeDefinition;

        public Vector3 WallDirection => (CornerBottomRight.position - CornerBottomLeft.position).normalized;

        public int CurrentLayerCount => _layers != null ? _layers.CurrentLayerCount : 0;

        protected SurfaceGeometryService _geometry;
        protected WallLayerService _layers;

        private readonly Dictionary<Transform, float> _sideOffsets = new();
        private readonly Dictionary<Transform, float> _verticalOffsets = new();

        private BoxCollider _collider;

        protected virtual void Awake()
        {
            _geometry = new SurfaceGeometryService(
                this,
                this.SurfaceType,
                transform,
                _cornerBottomLeft,
                _cornerBottomRight,
                _cornerTopLeft,
                _cornerTopRight
            );

            _layers = new WallLayerService(_allLayers, this);

            _collider = GetComponent<BoxCollider>();
            if(_collider == null)
            {
                _collider = gameObject.AddComponent<BoxCollider>();
            }

            _outline = new SurfaceOutline(transform, _collider, _outlineColor, _outlineWidth);
        }

        protected virtual void Start()
        {
            _roomManager = RoomManager.Instance;
            if (_roomManager != null)
            {
                _roomManager.RegisterSurface(this);
            }

            _layers.Reset();

            if (_initialMaterial != null)
            {
                _layers.ApplyInitialLayer(_initialMaterial);
            }

            UpdateGeometry();
        }

        public void SetHighlight(bool highlight)
        {
            if (highlight) _outline?.Show();
            else _outline?.Hide();
        }

        public virtual void UpdateGeometry()
        {
            if (_geometry == null) return;
            _geometry.Build(_allLayers, _layers.CurrentLayerCount);
            UpdateCollider();
            _outline?.UpdateMesh();
            
        }

        public void UpdateDimensionsAndGeometry()
        {
            UpdateGeometry();
        }        

        private void UpdateCollider()
        {
            if (_collider == null) return;

            float totalThickness = GetTotalThickness();
            Vector3[] corners = GetWorldCornersWithThickness(totalThickness);

            for (int i = 0; i < corners.Length; i++)
                corners[i] = transform.InverseTransformPoint(corners[i]);

            Vector3 min = corners[0];
            Vector3 max = corners[0];

            foreach (var p in corners)
            {
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }

            _collider.center = (min + max) * 0.5f;
            _collider.size = max - min;
        }        

        private Vector3[] GetWorldCornersWithThickness(float thickness)
        {
            Vector3 bl = _cornerBottomLeft.position;
            Vector3 br = _cornerBottomRight.position;
            Vector3 tl = _cornerTopLeft.position;
            Vector3 tr = _cornerTopRight.position;
            
            Vector3 normal = GetSurfaceNormal();
            if (SurfaceType == SurfaceType.Ceiling) normal = -normal;
            
            Vector3 bl_back = bl - normal * thickness;
            Vector3 br_back = br - normal * thickness;
            Vector3 tl_back = tl - normal * thickness;
            Vector3 tr_back = tr - normal * thickness;
            
            return new Vector3[] { bl, br, tl, tr, bl_back, br_back, tl_back, tr_back };
        }

        public List<MaterialDefinition> GetAllMaterialsFromLayers()
        {
            List<MaterialDefinition> materials = new List<MaterialDefinition>();
            
            if (_initialMaterial != null && !materials.Contains(_initialMaterial))
                materials.Add(_initialMaterial);
            
            foreach (var layer in _allLayers)
            {
                if (layer != null && layer.Material != null && !materials.Contains(layer.Material))
                    materials.Add(layer.Material);
            }
            
            return materials;
        }

        public void HideAllLayers()
        {
            _layers.Reset();
            _layers.NotifyThicknessRebuild();
            UpdateGeometry();
        }

        public void ResetSurface()
        {
            _layers.Reset();
            _layers.NotifyThicknessRebuild();
            UpdateGeometry();
            SurfaceEvents.RaiseSurfaceReset(this);
        }

        public void SetCornerOffset(Transform corner, float offset) => _sideOffsets[corner] = offset;
        public void SetVerticalOffset(Transform corner, float offset) => _verticalOffsets[corner] = offset;

        public void SetCorners(Transform bl, Transform br, Transform tl, Transform tr)
        {
            _cornerBottomLeft = bl;
            _cornerBottomRight = br;
            _cornerTopLeft = tl;
            _cornerTopRight = tr;
        }
        
        public void ClearOffsets()
        {
            _sideOffsets.Clear();
            _verticalOffsets.Clear();
        }

        public bool TryApplyMaterial(MaterialDefinition material) => _layers.TryApply(material, UpdateGeometry);
        public virtual Vector3 GetSurfaceNormal() => _geometry.GetNormal();
        public bool SharesCorner(Transform corner) => _geometry.SharesCorner(corner);                
        public float GetTotalThickness() => _layers.GetTotalThickness();
        public float GetCornerOffset(Transform corner) => _sideOffsets.TryGetValue(corner, out float value) ? value : 0;
        public float GetVerticalOffset(Transform corner) => _verticalOffsets.TryGetValue(corner, out float value) ? value : 0;
        public float GetLastLayerRemainingTime() => _layers.GetRemainingTime();
        public float GetLastLayerAdhesion() => _layers.GetAdhesion();
        public MaterialDefinition GetLastLayerMaterial() => _layers.GetLastMaterial();
        public AppliedLayerLogic[] GetAllLayers => _allLayers;

        protected virtual void OnDestroy()
        {
            if (_roomManager != null) _roomManager.UnregisterSurface(this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(_cornerBottomLeft.position, _cornerBottomRight.position);
            Gizmos.DrawLine(_cornerBottomRight.position, _cornerTopRight.position);
            Gizmos.DrawLine(_cornerTopRight.position, _cornerTopLeft.position);
            Gizmos.DrawLine(_cornerTopLeft.position, _cornerBottomLeft.position);
        }
    }
}