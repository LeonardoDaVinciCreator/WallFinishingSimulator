using UnityEngine;
using UnityEngine.UI;
using SurfaceSystem.Components;
using Core;

namespace SurfaceSystem.UI.SurfaceButton
{
    [RequireComponent(typeof(Button))]
    public class SurfaceButtonController : MonoBehaviour
    {
        [SerializeField] private SurfaceButtonView _view;
        private SurfaceButtonModel _model;
        
        public void Initialize(BaseSurfaceController surface, int number)
        {
            _model = new SurfaceButtonModel();
            _model.SetSurface(surface, number);
            
            if (_view == null)
                _view = GetComponent<SurfaceButtonView>();
            _view?.UpdateName(_model.SurfaceName);
            
            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(OnSurfaceClick);
        }

        private void OnSurfaceClick()
        {
            _model?.SelectSurface();
            SoundManager.Instance.PlaySurfaceSelect();
        }
    }
}