using System;
using System.Collections.Generic; 
using SurfaceSystem.Components;
using SurfaceSystem.Data;
using SurfaceSystem.Events;
using UnityEngine;

namespace SurfaceSystem.Core
{
    public class RoomManager : MonoBehaviour
    {
        private static RoomManager _instance;
        public static RoomManager Instance => _instance;

        [Header("Ссылки на все уникальные углы комнаты")]
        [SerializeField] private List<Transform> _bottomCorners = new();
        [SerializeField] private List<Transform> _topCorners = new();

        public List<Transform> BottomCorners => _bottomCorners;
        public List<Transform> TopCorners => _topCorners;

        private List<BaseSurfaceController> _surfaces = new();      

        private bool _thicknessChanged = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnEnable()
        {
            SurfaceEvents.OnCornerMoved += HandleCornerMoved;
            SurfaceEvents.OnSurfaceThicknessChanged += HandleSurfaceThicknessChanged;
        }

        private void OnDisable()
        {
            SurfaceEvents.OnCornerMoved -= HandleCornerMoved;
            SurfaceEvents.OnSurfaceThicknessChanged -= HandleSurfaceThicknessChanged;
        }

        private void LateUpdate()
        {
            if (_thicknessChanged)
            {
                _thicknessChanged = false;
                NotifySurfaceThicknessChanged(null, 0f);
            }
        }

        private void HandleSurfaceThicknessChanged(BaseSurfaceController changedSurface, float thickness)
        {
            //NotifySurfaceThicknessChanged(changedSurface, thickness);
            _thicknessChanged = true;
        }

        private void HandleCornerMoved(Transform corner)
        {
            UpdateAllSurfaces();
        }

        public void RegisterSurface(BaseSurfaceController surface)
        {
            if (!_surfaces.Contains(surface))
            {
                _surfaces.Add(surface);
                SurfaceEvents.RaiseSurfaceRegistered(surface);
            }
            UpdateAllSurfaces();
        }

        public void UnregisterSurface(BaseSurfaceController wall)
        {
            if(_surfaces.Contains(wall)) _surfaces.Remove(wall);
        }
        
        public void MoveCorner(Transform corner, Vector3 newPosition)
        {
            Vector3 oldPos = corner.position;
            float eps = 0.01f;
            foreach (var surface in _surfaces)
            {
                foreach (var c in new[] { surface.CornerBottomLeft, 
                surface.CornerBottomRight, surface.CornerTopLeft, 
                surface.CornerTopRight })
                {
                    if (c != null && Vector3.Distance(c.position, oldPos) < eps)
                        c.position = newPosition;
                }
            }
            
            _thicknessChanged = true;
            SurfaceEvents.RaiseCornerMoved(corner);
        }

        public void UpdateAllSurfaces()
        {
            foreach (var wall in _surfaces)
            {
                wall.UpdateDimensionsAndGeometry();
            }
        }

        public List<BaseSurfaceController> GetAllSurfaces() => _surfaces;        

        public void NotifySurfaceThicknessChanged(
            BaseSurfaceController changedSurface,
            float thickness)
        {
            if (_surfaces == null)
                return;
            
            foreach (var s in _surfaces)
            {
                s.ClearOffsets();
            }
           
            // перерасчет всех стыков
            foreach (var source in _surfaces)
            {
                bool sourceIsFloor = source.SurfaceType == SurfaceType.Floor;
                bool sourceIsCeiling = source.SurfaceType == SurfaceType.Ceiling;
                bool sourceIsWall = source.SurfaceType == SurfaceType.Wall;

                float sourceThickness =
                    source.GetTotalThickness();

                foreach (var target in _surfaces)
                {
                    if (source == target) continue;

                    bool targetIsFloor = target.SurfaceType == SurfaceType.Floor;
                    bool targetIsCeiling = target.SurfaceType == SurfaceType.Ceiling;
                    bool targetIsWall =
                        !targetIsFloor && !targetIsCeiling;
                    
                    //стены
                    if (sourceIsWall && targetIsWall)
                    {
                        float dot = Vector3.Dot(source.WallDirection, target.WallDirection);
                        bool straight = dot > 0.99f;
                        float offset = straight ? 0f : sourceThickness;

                        ApplyCornerOffset(source, target, offset);
                    }

                    //пол и стены
                    else if (sourceIsFloor && targetIsWall)
                    {
                        ApplyVerticalOffsetBottom(source, target, sourceThickness);
                    }

                    // потолок и стены
                    else if (sourceIsCeiling && targetIsWall)
                    {
                        ApplyVerticalOffsetTop(source, target, sourceThickness);
                    }
                }
            }
            
            foreach (var s in _surfaces)
            {
                s.UpdateDimensionsAndGeometry();
            }
        }

        private void ApplyCornerOffset(
            BaseSurfaceController source,
            BaseSurfaceController target,
            float offset)
        {
            if (source.SharesCorner(target.CornerBottomLeft))
                target.SetCornerOffset(
                    target.CornerBottomLeft,
                    offset);

            if (source.SharesCorner(target.CornerBottomRight))
                target.SetCornerOffset(
                    target.CornerBottomRight,
                    offset);

            if (source.SharesCorner(target.CornerTopLeft))
                target.SetCornerOffset(
                    target.CornerTopLeft,
                    offset);

            if (source.SharesCorner(target.CornerTopRight))
                target.SetCornerOffset(
                    target.CornerTopRight,
                    offset);
        }

        private void ApplyVerticalOffsetBottom(
            BaseSurfaceController source,
            BaseSurfaceController target,
            float offset)
        {
            if (source.SharesCorner(target.CornerBottomLeft))
                target.SetVerticalOffset(
                    target.CornerBottomLeft,
                    offset);

            if (source.SharesCorner(target.CornerBottomRight))
                target.SetVerticalOffset(
                    target.CornerBottomRight,
                    offset);
        }

        private void ApplyVerticalOffsetTop(
            BaseSurfaceController source,
            BaseSurfaceController target,
            float offset)
        {
            if (source.SharesCorner(target.CornerTopLeft))
                target.SetVerticalOffset(
                    target.CornerTopLeft,
                    offset);

            if (source.SharesCorner(target.CornerTopRight))
                target.SetVerticalOffset(
                    target.CornerTopRight,
                    offset);
        }
    }
}