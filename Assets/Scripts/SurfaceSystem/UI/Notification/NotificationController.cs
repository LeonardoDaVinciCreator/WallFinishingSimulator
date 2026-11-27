using UnityEngine;
using SurfaceSystem.Events;
using SurfaceSystem.Components;
using SurfaceSystem.Data;
using SurfaceSystem.Utils;

namespace SurfaceSystem.UI.Notification
{
    public class NotificationController : MonoBehaviour
    {
        [SerializeField] private NotificationView _notificationView;

        private void Start()
        {
            if (_notificationView == null)
                _notificationView = FindObjectOfType<NotificationView>();
            
            SurfaceEvents.OnLayerApplied += OnLayerApplied;
            SurfaceEvents.OnLayerError += OnLayerError;
            SurfaceEvents.OnSurfaceReset += OnSurfaceReset;
            SurfaceEvents.OnSurfaceCompleted += OnSurfaceCompleted;
            SurfaceEvents.OnNotification += OnNotification;
        }

        private void OnLayerApplied(BaseSurfaceController surface, MaterialDefinition material)
        {
            if (material.IsFinalLayer)
                return;

            string message = $"Успешно! Слой {material.DisplayName} нанесён на {GetSurfaceName(surface)}";
            _notificationView?.Show(message, true);
        }

        private void OnLayerError(BaseSurfaceController surface, string error)
        {
            string message = $"Ой, Ошибка... {error}";
            _notificationView?.Show(message, false);
        }

        private void OnSurfaceReset(BaseSurfaceController surface)
        {
            string message = LayerOrderValidator.GetResetMessage(surface);
            _notificationView?.Show(message, true);
        }

        private void OnSurfaceCompleted(BaseSurfaceController surface)
        {
            string message = $"Молодец, поверхность {GetSurfaceName(surface)} полностью обработана!";
            _notificationView?.Show(message, true);
        }

        private string GetSurfaceName(BaseSurfaceController surface)
        {
            switch (surface.SurfaceType)
            {
                case SurfaceType.Wall: return "стену";
                case SurfaceType.Floor: return "пол";
                case SurfaceType.Ceiling: return "потолок";
                default: return "поверхность";
            }
        }

        private void OnNotification(string message)
        {
            _notificationView?.Show(message, true);
        }

        private void OnDestroy()
        {
            SurfaceEvents.OnLayerApplied -= OnLayerApplied;
            SurfaceEvents.OnLayerError -= OnLayerError;
            SurfaceEvents.OnSurfaceReset -= OnSurfaceReset;
            SurfaceEvents.OnSurfaceCompleted -= OnSurfaceCompleted;
            SurfaceEvents.OnNotification -= OnNotification;
        }
    }
}