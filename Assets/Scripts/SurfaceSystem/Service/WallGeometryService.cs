using UnityEngine;
using SurfaceSystem.Utils;
using SurfaceSystem.Components;
using SurfaceSystem.Data;

namespace SurfaceSystem.Service
{
    public class SurfaceGeometryService
    {
        private readonly Transform _t;

        private readonly Transform _bl;
        private readonly Transform _br;
        private readonly Transform _tl;
        private readonly Transform _tr;

        private readonly BaseSurfaceController _surface;
        private readonly SurfaceType _surfaceType;

        public SurfaceGeometryService(
            BaseSurfaceController surface,
            SurfaceType surfaceType,
            Transform t,
            Transform bl,
            Transform br,
            Transform tl,
            Transform tr)
        {
            _surface = surface;
            _surfaceType = surfaceType;

            _t = t;

            _bl = bl;
            _br = br;
            _tl = tl;
            _tr = tr;
        }

        public void Build(
            AppliedLayerLogic[] layers,
            int layerCount)
        {
            BuildBaseSurface();            

            float accumulatedThickness = 0f;

            for (int i = 0; i < layerCount; i++)
            {
                var layer = layers[i];

                if (layer == null) continue;

                if (!layer.gameObject.activeSelf) continue;

                if (!layer.TryGetComponent<MeshFilter>(out var mf)) continue;

                float layerThickness = layer.GetThickness();

                Vector3[] front = GetAdjustedCorners(accumulatedThickness);

                Vector3[] back = GetAdjustedCorners(accumulatedThickness + layerThickness);

                mf.transform.localPosition = Vector3.zero;

                mf.transform.localRotation = Quaternion.identity;

                mf.mesh = WallMeshBuilder.GenerateWallMesh(front, back);

                accumulatedThickness += layerThickness;
            }
        }

        private void BuildBaseSurface()
        {
            var meshFilter = _t.GetComponent<MeshFilter>();

            if (meshFilter == null) return;

            Mesh mesh = new Mesh();

            Vector3[] vertices =
            {
                _t.InverseTransformPoint(_bl.position),
                _t.InverseTransformPoint(_br.position),
                _t.InverseTransformPoint(_tl.position),
                _t.InverseTransformPoint(_tr.position)
            };

            int[] triangles =
            {
                0, 2, 1,
                2, 3, 1
            };

            Vector2[] uv =
            {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1)
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
        }

        public Vector3 GetNormal()
        {
            Vector3 a = _bl.position;
            Vector3 b = _br.position;
            Vector3 c = _tl.position;

            return Vector3.Cross(b - a, c - a).normalized;
        }

        public bool SharesCorner(
            Transform corner)
        {
            if (corner == null) return false;

            float eps = 0.05f;

            return
                Vector3.Distance(_bl.position, corner.position) < eps ||
                Vector3.Distance(_br.position, corner.position) < eps ||
                Vector3.Distance(_tl.position, corner.position) < eps ||
                Vector3.Distance(_tr.position, corner.position) < eps;
        }

        private Vector3[] GetAdjustedCorners(
            float depthOffset)
        {
            Vector3 normal = GetNormal();
            if (_surfaceType == SurfaceType.Ceiling)
                normal = -normal;

            Vector3 horizontal = (_br.position - _bl.position).normalized;

            Vector3 vertical = (_tl.position - _bl.position).normalized;

            if (_surfaceType == SurfaceType.Floor) 
                vertical = -vertical;

            float blSide = _surface.GetCornerOffset(_bl);
            float brSide = _surface.GetCornerOffset(_br);
            float tlSide = _surface.GetCornerOffset(_tl);
            float trSide = _surface.GetCornerOffset(_tr);

            float blVert = _surface.GetVerticalOffset(_bl);
            float brVert = _surface.GetVerticalOffset(_br);
            float tlVert = _surface.GetVerticalOffset(_tl);
            float trVert = _surface.GetVerticalOffset(_tr);            
            
            Vector3 logicalBL = _bl.position + horizontal * blSide + vertical * blVert;
            Vector3 logicalBR = _br.position - horizontal * brSide + vertical * brVert; 
            Vector3 logicalTL = _tl.position + horizontal * tlSide - vertical * tlVert;
            Vector3 logicalTR = _tr.position - horizontal * trSide - vertical * trVert;
            
            Vector3 bl = logicalBL - normal * depthOffset;
            Vector3 br = logicalBR - normal * depthOffset;
            Vector3 tl = logicalTL - normal * depthOffset;
            Vector3 tr = logicalTR - normal * depthOffset;

            return new[]
            {
                _t.InverseTransformPoint(bl),
                _t.InverseTransformPoint(br),
                _t.InverseTransformPoint(tl),
                _t.InverseTransformPoint(tr),
            };
        }
    }
}