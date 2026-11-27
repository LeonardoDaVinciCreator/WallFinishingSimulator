using SurfaceSystem.Utils;
using UnityEngine;

namespace SurfaceSystem.Components
{
    public class SurfaceOutline
    {
        private Color _color = Color.yellow;
        private float _width = 0.05f;
        
        private GameObject _outlineObject;
        private LineRenderer _lineRenderer;
        private BoxCollider _collider;
        private Transform _parent;

        public SurfaceOutline(Transform parent, BoxCollider collider, Color color, float width)
        {
            _parent = parent;
            _collider = collider;
            _color = color;
            _width = width;
            CreateOutline();
        }

        private void CreateOutline()
        {
            _outlineObject = new GameObject("Outline");
            _outlineObject.transform.SetParent(_parent);
            _outlineObject.transform.localPosition = Vector3.zero;
            _outlineObject.transform.localRotation = Quaternion.identity;
            _outlineObject.transform.localScale = Vector3.one;
            _outlineObject.layer = LayerMask.NameToLayer("Outline");
            
            _lineRenderer = _outlineObject.AddComponent<LineRenderer>();
            _lineRenderer.startWidth = _width;
            _lineRenderer.endWidth = _width;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            
            _lineRenderer.startColor = _color;
            _lineRenderer.endColor = _color;
            _lineRenderer.useWorldSpace = true;
            
            Hide();
        }

        public void UpdateMesh()
        {
            if (_lineRenderer == null || _collider == null) return;
            
            Vector3 center = _collider.center;
            Vector3 size = _collider.size;
            
            Vector3[] localCorners = SurfaceGeometryHelper.GetBoxCorners(center, size);
            Vector3[] worldCorners = SurfaceGeometryHelper.ToWorldSpace(localCorners, _parent);
            Vector3[] edges = SurfaceGeometryHelper.GetBoxEdges(worldCorners);
            
            _lineRenderer.positionCount = edges.Length;
            _lineRenderer.SetPositions(edges);
        }

        public void Show() => _outlineObject?.SetActive(true);
        public void Hide() => _outlineObject?.SetActive(false);
        public bool IsActive() => _outlineObject != null && _outlineObject.activeSelf;
    }
}