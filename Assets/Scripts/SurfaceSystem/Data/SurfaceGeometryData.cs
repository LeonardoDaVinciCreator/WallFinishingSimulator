using UnityEngine;

namespace SurfaceSystem.Data
{
    public struct SurfaceGeometryData
    {
        public Vector3 BottomLeft;
        public Vector3 BottomRight;
        public Vector3 TopLeft;
        public Vector3 TopRight;

        public Vector3 Normal;

        public float Thickness;
    }
}