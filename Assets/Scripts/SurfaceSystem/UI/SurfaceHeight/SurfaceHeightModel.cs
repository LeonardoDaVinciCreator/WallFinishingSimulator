using System.Collections.Generic;
using UnityEngine;
using SurfaceSystem.Core;

namespace SurfaceSystem.UI.SurfaceHeight
{
    public class SurfaceHeightModel
    {
        private List<Transform> _bottomCorners = new();
        private List<Transform> _topCorners = new();
        private SurfaceHeightService _service;

        public bool IsTopSelected { get; set; } = false;

        public SurfaceHeightModel(SurfaceHeightService service)
        {
            _service = service;
            var roomManager = RoomManager.Instance;
            if (roomManager != null)
            {
                _bottomCorners.AddRange(roomManager.BottomCorners);
                _topCorners.AddRange(roomManager.TopCorners);
            }
        }

        public void ApplyOffset(float heightOffset)
        {
            _service.SetCurrentOffset(heightOffset);

            if (IsTopSelected)
            {
                foreach (var corner in _topCorners)
                {
                    corner.position += new Vector3(0, heightOffset, 0);
                }
                _service.AddTopOffset(heightOffset);
            }
            else
            {
                foreach (var corner in _bottomCorners)
                {
                    corner.position += new Vector3(0, heightOffset, 0);
                }
                _service.AddBottomOffset(heightOffset);
            }
        }

        public void Cancel()
        {
            float topOffset = _service.TopOffset;
            float bottomOffset = _service.BottomOffset;

            foreach (var corner in _topCorners)
            {
                corner.position -= new Vector3(0, topOffset, 0);
            }
            foreach (var corner in _bottomCorners)
            {
                corner.position -= new Vector3(0, bottomOffset, 0);
            }
            
            _service.ResetOffsets();
        }

        public void UpdateAllSurfaces()
        {
            RoomManager.Instance?.UpdateAllSurfaces();
        }
    }
}