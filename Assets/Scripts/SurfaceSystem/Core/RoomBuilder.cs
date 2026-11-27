using UnityEngine;
using System.Collections.Generic;
using SurfaceSystem.Components;

namespace SurfaceSystem.Core
{
    [ExecuteInEditMode]
    public class RoomBuilder : MonoBehaviour
    {
        [Header("Контейнеры с точками по часовой стрелке")]
        public Transform bottomContainer;
        public Transform topContainer;

        [Header("Все нижние точки (построчно, слева направо)")]
        public Transform bottomRowsContainer;

        [Header("Все верхние точки (построчно, слева направо)")]
        public Transform topRowsContainer;

        [Header("Префабы")]        
        public GameObject ceilingPrefab;
        public GameObject wallPrefab;
        public GameObject floorPrefab;

        [Header("Родительские объекты для групп")]        
        public Transform ceilingParent;
        public Transform wallsParent;
        public Transform floorParent;

        [ContextMenu("Построить всю комнату")]
        public void BuildFullRoom()
        {
            BuildWall();
            BuildFloor();
            BuildCeiling();
            Debug.Log("комната построена!");
        }

        [ContextMenu("Постоить стены")]
        public void BuildWall()
        {
            //пройтить по часовой стрелке и втсавить значения

            if (bottomContainer == null || topContainer == null)
            {
                Debug.LogError("Назначьте контейнеры для стен!");
                return;
            }

            List<Transform> bottomPoints = new List<Transform>();
            List<Transform> topPoints = new List<Transform>();
            foreach (Transform child in bottomContainer) bottomPoints.Add(child);
            foreach (Transform child in topContainer) topPoints.Add(child);

            if (bottomPoints.Count < 3 || topPoints.Count != bottomPoints.Count)
            {
                Debug.LogError($"Ошибка: нижних {bottomPoints.Count}, верхних {topPoints.Count}. Нужно минимум 3 и одинаковое количество.");
                return;
            }

            for(int i=0; i<bottomPoints.Count; i++)
            {
                int next = (i+1) % bottomPoints.Count;
                CreateWall(bottomPoints[i], bottomPoints[next], topPoints[i], topPoints[next]);
            }

            Debug.Log($"Стены построены: {bottomPoints.Count} шт.");
        }

        private void BuildFloorOrCeiling(bool isCeiling, string debugName)
        {
            GameObject prefab = isCeiling ? ceilingPrefab : floorPrefab;
            Transform rowsContainer = isCeiling ? topRowsContainer : bottomRowsContainer;
            Transform parent = isCeiling ? ceilingParent : floorParent;
            string objectName = isCeiling ? "Ceiling" : "Floor";

            if (prefab == null)
            {
                Debug.LogError($"{debugName} Prefab не назначен!");
                return;
            }

            if (rowsContainer == null)
            {
                Debug.LogError($"Назначьте для {debugName} RowsContainer!");
                return;
            }

            List<List<Transform>> rows = GetRows(rowsContainer);

            if (rows.Count < 2)
            {
                Debug.LogError($"Нужно минимум 2 строки для {debugName}!");
                return;
            }

            int expectedCols = rows[0].Count;
            for (int i = 1; i < rows.Count; i++)
            {
                if (rows[i].Count != expectedCols)
                {
                    Debug.LogError($"Строка {i} содержит {rows[i].Count} точек, а строка 0 содержит {expectedCols}. Количество должно совпадать!");
                    return;
                }
            }

            if (expectedCols < 2)
            {
                Debug.LogError($"В каждой строке должно быть минимум 2 точки для {debugName}!");
                return;
            }

            if (parent == null)
            {
                Debug.LogError($"Не назначен родителл для {debugName}");
                return;
            }

            for (int row = 0; row < rows.Count - 1; row++)
            {
                List<Transform> currentRow = rows[row];
                List<Transform> nextRow = rows[row + 1];

                for (int col = 0; col < expectedCols - 1; col++)
                {
                    Transform bl = currentRow[col];
                    Transform br = currentRow[col + 1];
                    Transform tl = nextRow[col];
                    Transform tr = nextRow[col + 1];

                    if (!isCeiling)
                    {
                        Transform tempBl = bl;
                        Transform tempBr = br;
                        bl = tl;
                        br = tr;
                        tl = tempBl;
                        tr = tempBr;
                    }

                    CreateFloorSegment(bl, br, tl, tr, isCeiling, parent);
                }
            }

            Debug.Log($"{debugName} построен.");
        }
        
        [ContextMenu("Построить пол")]
        public void BuildFloor()
        {
            BuildFloorOrCeiling(false, "пол");
        }

        [ContextMenu("Построить потолок")]
        public void BuildCeiling()
        {
            BuildFloorOrCeiling(true, "потолок");
        }

        void CreateWall(Transform bl, Transform br, Transform tl, Transform tr)
        {
            if (wallPrefab == null)
            {
                Debug.LogError("wallPrefab не назначен!");
                return;
            }

            GameObject wall = Instantiate(wallPrefab, wallsParent ?? transform);
            wall.name = $"Wall_{bl.name}_{br.name}";

            var controller = wall.GetComponent<BaseSurfaceController>();
            if(controller == null)
            {
                Debug.LogError("Нет BaseSurfaceController у стены");
                DestroyImmediate(wall);
                return;
            }

            controller.SetCorners(bl, br, tl, tr);
        }

        private void CreateFloorSegment(Transform bl, Transform br, Transform tl, Transform tr, bool isCeiling, Transform parent)
        {
            GameObject prefab = isCeiling ? ceilingPrefab : floorPrefab;
            if (prefab == null) return;

            GameObject segment = Instantiate(prefab, parent);
            segment.name = (isCeiling ? "Ceiling" : "Floor") + $"_{bl.name}_{br.name}_{tl.name}_{tr.name}";

            var controller = segment.GetComponent<BaseSurfaceController>();
            if (controller == null)
            {
                Debug.LogError($"Префаб {(isCeiling ? "потолка" : "пола")} не содержит BaseSurfaceController!");
                DestroyImmediate(segment);
                return;
            }

            if (isCeiling)
            {
                controller.SetCorners(tl, tr, bl, br);
            }
            else
            {
                controller.SetCorners(bl, br, tl, tr);
            }
        }

        private List<List<Transform>> GetRows(Transform container)
        {
            List<List<Transform>> rows = new List<List<Transform>>();
            foreach (Transform row in container)
            {
                List<Transform> points = new List<Transform>();
                foreach (Transform point in row)
                {
                    points.Add(point);
                }
                if (points.Count > 0)
                    rows.Add(points);
            }
            return rows;
        }
    }
}