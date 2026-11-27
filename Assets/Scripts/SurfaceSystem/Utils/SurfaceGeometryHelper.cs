using UnityEngine;

namespace SurfaceSystem.Utils
{
    public static class SurfaceGeometryHelper
    {
        public static Vector3[] GetBoxCorners(Vector3 center, Vector3 size)
        {
            Vector3 half = size * 0.5f;
            return new Vector3[]
            {
                center + new Vector3(-half.x, -half.y, -half.z),
                center + new Vector3( half.x, -half.y, -half.z),
                center + new Vector3(-half.x,  half.y, -half.z),
                center + new Vector3( half.x,  half.y, -half.z),
                center + new Vector3(-half.x, -half.y,  half.z),
                center + new Vector3( half.x, -half.y,  half.z),
                center + new Vector3(-half.x,  half.y,  half.z),
                center + new Vector3( half.x,  half.y,  half.z)
            };
        }

        public static Vector3[] ToWorldSpace(Vector3[] localPoints, Transform parent)
        {
            Vector3[] world = new Vector3[localPoints.Length];
            for (int i = 0; i < localPoints.Length; i++)
                world[i] = parent.TransformPoint(localPoints[i]);
            return world;
        }

        public static Vector3[] GetBoxEdges(Vector3[] corners)
        {
            return new Vector3[]
            {
                corners[0], corners[1], corners[1], corners[3],
                corners[3], corners[2], corners[2], corners[0],
                corners[4], corners[5], corners[5], corners[7],
                corners[7], corners[6], corners[6], corners[4],
                corners[0], corners[4], corners[1], corners[5],
                corners[2], corners[6], corners[3], corners[7]
            };
        }
    }
}