using SurfaceSystem.Components;
using SurfaceSystem.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SurfaceSystem.Core
{
    public class SurfaceClickDetector : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private float _distance = 1000f;
        [SerializeField] private LayerMask _clickableLayers;
        
        private void Awake()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;
                
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, _distance, _clickableLayers))
                {
                    BaseSurfaceController surface = hit.collider.GetComponent<BaseSurfaceController>();
                    if (surface != null && RepairUIManager.Instance != null)
                    {
                        RepairUIManager.Instance.SelectWall(surface);
                        RepairUIManager.Instance.ShowMaterialPanel(surface.SurfaceType);
                    }
                }
            }
        }
    }
}