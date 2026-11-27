using UnityEngine;
using UnityEngine.UI;
using SurfaceSystem.Data;
using Core;

namespace SurfaceSystem.UI.MaterialButton
{
    [RequireComponent(typeof(Button))]
    public class MaterialButtonController : MonoBehaviour
    {
        [SerializeField] private MaterialButtonView _view;
        
        private MaterialButtonModel _model;

        public void Initialize(MaterialDefinition material)
        {
            _model = new MaterialButtonModel();

            _model.SetMaterial(material);
            _view.UpdateName(_model.DisplayName);
            
            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(OnButtonClick);                
        }

        private void OnButtonClick()
        {
            _model.ApplyMaterial();
            SoundManager.Instance.PlaySurfaceSelect();
            Debug.Log($"Материал {_model.DisplayName}, применяется на поверхность.");
        }
    }
}