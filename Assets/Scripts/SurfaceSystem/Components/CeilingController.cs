using UnityEngine;

namespace SurfaceSystem.Components
{
    public class CeilingController : BaseSurfaceController
    {
        private MeshFilter[] _meshFilters;
        // испрвляет для потолка поворот граней найзнаку

        protected override void Awake()
        {
            base.Awake();
            _meshFilters = GetComponentsInChildren<MeshFilter>();
        }

        public override void UpdateGeometry()
        {        
            base.UpdateGeometry();            

            foreach (var meshFilter in _meshFilters)
            {
                Mesh mesh = meshFilter.sharedMesh;
                if (mesh == null) continue;

                int[] triangles = mesh.triangles;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int temp = triangles[i + 1];
                    triangles[i + 1] = triangles[i + 2];
                    triangles[i + 2] = temp;
                }
                mesh.triangles = triangles;

                mesh.RecalculateNormals();
            }
        }
    }
}