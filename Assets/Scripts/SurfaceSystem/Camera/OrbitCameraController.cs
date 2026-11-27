using UnityEngine;
using UnityEngine.EventSystems;

namespace SurfaceSystem.CameraControl
{
    public class OrbitCameraController : MonoBehaviour
    {
        [Header("Target & Framing")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = Vector3.zero;

        [Header("Orbit Settings")]
        [SerializeField] private bool useRightMouseButton = true;
        [SerializeField] private float rotationSpeedX = 320f;
        [SerializeField] private float rotationSpeedY = 210f;
        [SerializeField] private float smoothTime = 0.04f;

        [Header("Angle Limits")]
        [SerializeField] private float minVerticalAngle = -10f;
        [SerializeField] private float maxVerticalAngle = 88f;
        [SerializeField] private bool clampHorizontalAngle = false;
        [SerializeField] private float minHorizontalAngle = -180f;
        [SerializeField] private float maxHorizontalAngle = 180f;

        [Header("Zoom Settings")]
        [SerializeField] private float distance = 7f;
        [SerializeField] private float minDistance = 0.8f;
        [SerializeField] private float maxDistance = 25f;
        [SerializeField] private float zoomSpeed = 8f;

        [Header("Pan Settings (Optional)")]
        [SerializeField] private bool allowPanning = true;
        [SerializeField] private float panSpeed = 0.8f;
        [SerializeField] private Vector2 panBoundsX = new Vector2(-4f, 4f);
        [SerializeField] private Vector2 panBoundsZ = new Vector2(-4f, 4f);

        private float _currentYaw;
        private float _currentPitch;
        private float _targetYaw;
        private float _targetPitch;
        private float _yawVelocity;
        private float _pitchVelocity;

        private float _currentDistance;
        private float _distanceVelocity;

        private bool _isDraggingOrbit;
        private bool _isDraggingPan;
        private bool _isRotationPaused;

        private int RotateMouseButton => useRightMouseButton ? 1 : 0;
        private int PanMouseButton => useRightMouseButton ? 0 : 1;

        private void Start()
        {
            SyncWithCurrentTransform();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            HandleInputState();
            HandlePanning();
            HandleZoom();
            HandleOrbit();

            ApplyCameraTransform();
        }

        private void HandleInputState()
        {
            if (Input.GetMouseButtonDown(RotateMouseButton) && !_isRotationPaused && !IsPointerOverUI())
            {
                _isDraggingOrbit = true;
            }
            if (Input.GetMouseButtonUp(RotateMouseButton))
            {
                _isDraggingOrbit = false;
            }

            if (allowPanning && Input.GetMouseButtonDown(PanMouseButton) && !_isRotationPaused && !IsPointerOverUI())
            {
                _isDraggingPan = true;
            }
            if (Input.GetMouseButtonUp(PanMouseButton))
            {
                _isDraggingPan = false;
            }
        }

        private void HandleOrbit()
        {
            if (_isDraggingOrbit && !_isRotationPaused)
            {
                float dt = Time.unscaledDeltaTime;
                _targetYaw += Input.GetAxis("Mouse X") * rotationSpeedX * dt;
                _targetPitch -= Input.GetAxis("Mouse Y") * rotationSpeedY * dt;
            }

            _targetPitch = Mathf.Clamp(_targetPitch, minVerticalAngle, maxVerticalAngle);

            if (clampHorizontalAngle)
            {
                _targetYaw = Mathf.Clamp(_targetYaw, minHorizontalAngle, maxHorizontalAngle);
            }

            _currentYaw = Mathf.SmoothDamp(_currentYaw, _targetYaw, ref _yawVelocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            _currentPitch = Mathf.SmoothDamp(_currentPitch, _targetPitch, ref _pitchVelocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        }

        private void HandleZoom()
        {
            if (_isRotationPaused || IsPointerOverUI()) return;

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                // Адаптивный шаг зума: быстрее на дальнем расстоянии и точнее вблизи
                float dynamicStep = zoomSpeed * Mathf.Max(0.4f, _currentDistance * 0.25f);
                distance = Mathf.Clamp(distance - scroll * dynamicStep, minDistance, maxDistance);
            }

            _currentDistance = Mathf.SmoothDamp(_currentDistance, distance, ref _distanceVelocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        }

        private void HandlePanning()
        {
            if (!allowPanning || !_isDraggingPan || _isRotationPaused) return;

            float dt = Time.unscaledDeltaTime;
            float moveX = -Input.GetAxis("Mouse X") * panSpeed * dt * _currentDistance;
            float moveZ = -Input.GetAxis("Mouse Y") * panSpeed * dt * _currentDistance;

            Vector3 pos = target.position + new Vector3(moveX, 0f, moveZ);
            pos.x = Mathf.Clamp(pos.x, panBoundsX.x, panBoundsX.y);
            pos.z = Mathf.Clamp(pos.z, panBoundsZ.x, panBoundsZ.y);
            target.position = pos;
        }

        private void ApplyCameraTransform()
        {
            Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
            Vector3 pivot = target.position + (targetOffset * _currentDistance);
            Vector3 position = pivot - (rotation * Vector3.forward * _currentDistance);

            transform.SetPositionAndRotation(position, rotation);
        }

        private void SyncWithCurrentTransform()
        {
            Vector3 initialAngles = transform.eulerAngles;
            _currentYaw = _targetYaw = NormalizeAngle(initialAngles.y);
            _currentPitch = _targetPitch = Mathf.Clamp(NormalizeAngle(initialAngles.x), minVerticalAngle, maxVerticalAngle);
            _currentDistance = distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        private static bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        public void SetRotationPaused(bool isPaused)
        {
            _isRotationPaused = isPaused;
            if (isPaused)
            {
                _isDraggingOrbit = false;
                _isDraggingPan = false;
            }
        }

        public void SetDistance(float newDistance)
        {
            distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                SyncWithCurrentTransform();
            }
        }
    }
}
