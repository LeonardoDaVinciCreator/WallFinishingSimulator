using UnityEngine;
using SurfaceSystem.Data;

namespace SurfaceSystem.Utils
{
    public static class WallMeshBuilder
    {

        public static Mesh GenerateWallMesh(Vector3[] baseCorners, Vector3[] extrudedCorners)
        {
            // на каждую грань по 4 отдельные вершины, итого вместо 8 вершин стало 4 вершины * 6 граней = 24 вершины
            var vertices = new Vector3[24];
            // u - ось x(ширина), v - ось y(высота), координаты как на обычной декартовой системе координат, но только 1 четверть
            var uv = new Vector2[24];            
            var triangles = new int[36];            
            
            Vector3 size = new Vector3(
                Vector3.Distance(baseCorners[1], baseCorners[0]),   // x: ширина
                Vector3.Distance(baseCorners[2], baseCorners[0]),   // y: высота
                Vector3.Distance(extrudedCorners[0], baseCorners[0]) // z: толщина
            );
            
            float texRepeat = 0.7f;//чем меньше значение тем меньше текстура
            
            // передняя грань U = ширина X, V = высота Y
            AddQuad(vertices, uv, triangles, 0,
                baseCorners[0], baseCorners[1], baseCorners[3], baseCorners[2],
                new Vector2(0, 0), 
                new Vector2(size.x / texRepeat, 0),
                new Vector2(size.x / texRepeat, size.y / texRepeat),
                new Vector2(0, size.y / texRepeat));
            
            // задняя грань U = X, V = Y
            // точки по часовой стрелке них лево до вверх лево
            AddQuad(vertices, uv, triangles, 4,
                extrudedCorners[1], extrudedCorners[0], extrudedCorners[2], extrudedCorners[3],
                new Vector2(0, 0), 
                new Vector2(size.x / texRepeat, 0),
                new Vector2(size.x / texRepeat, size.y / texRepeat), 
                new Vector2(0, size.y / texRepeat));

            
            // нижняя грань  пповорот 90: U = глубина Z, V = ширина X
           AddQuad(vertices, uv, triangles, 8,
                baseCorners[0], extrudedCorners[0], extrudedCorners[1], baseCorners[1],
                new Vector2(0, 0), 
                new Vector2(size.z / texRepeat, 0),
                new Vector2(size.z / texRepeat, size.x / texRepeat), 
                new Vector2(0, size.x / texRepeat));

            // верхняя грань поворот 90 также как и нижняя грань: U = глубина Z, V = ширина X
            AddQuad(vertices, uv, triangles, 12,
                baseCorners[2], baseCorners[3], extrudedCorners[3], extrudedCorners[2],
                new Vector2(0, 0), 
                new Vector2(0, size.x / texRepeat),
                new Vector2(size.z / texRepeat, size.x / texRepeat), 
                new Vector2(size.z / texRepeat, 0));
            
            // левая грань U = глубина Z, V = высота Y
            AddQuad(vertices, uv, triangles, 16,
                baseCorners[0], baseCorners[2], extrudedCorners[2], extrudedCorners[0],
                new Vector2(0, 0), 
                new Vector2(0, size.y / texRepeat),
                new Vector2(size.z / texRepeat, size.y / texRepeat), 
                new Vector2(size.z / texRepeat, 0));

            //   правая грань U = глубина Z, v = высота Y
            AddQuad(vertices, uv, triangles, 20,
                extrudedCorners[1], extrudedCorners[3], baseCorners[3], baseCorners[1],
                new Vector2(0, 0), 
                new Vector2(0, size.y / texRepeat),
                new Vector2(size.z / texRepeat, size.y / texRepeat), 
                new Vector2(size.z / texRepeat, 0));
            
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds(); 
            
            return mesh;
        }

        private static void AddQuad(
            Vector3[] vertices, Vector2[] uv, int[] triangles,
            int vertexOffset, 
            Vector3 a, 
            Vector3 b, 
            Vector3 c, 
            Vector3 d, 
            Vector2 uvA, 
            Vector2 uvB, 
            Vector2 uvC, 
            Vector2 uvD)
        {
            vertices[vertexOffset + 0] = a;
            vertices[vertexOffset + 1] = b;
            vertices[vertexOffset + 2] = c;
            vertices[vertexOffset + 3] = d;
                
            uv[vertexOffset + 0] = uvA;
            uv[vertexOffset + 1] = uvB;
            uv[vertexOffset + 2] = uvC;
            uv[vertexOffset + 3] = uvD;
                
            int trianglesOffset = (vertexOffset / 4) * 6;
            triangles[trianglesOffset + 0] = vertexOffset + 0;
            triangles[trianglesOffset + 1] = vertexOffset + 1;
            triangles[trianglesOffset + 2] = vertexOffset + 2;
            triangles[trianglesOffset + 3] = vertexOffset + 0;
            triangles[trianglesOffset + 4] = vertexOffset + 2;
            triangles[trianglesOffset + 5] = vertexOffset + 3;
        }
        
        public static Mesh GenerateWallMesh(SurfaceGeometryData data)
        {
            return new Mesh();
        }
    }
}