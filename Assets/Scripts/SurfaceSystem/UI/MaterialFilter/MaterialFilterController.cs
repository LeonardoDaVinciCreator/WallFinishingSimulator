using SurfaceSystem.Events;
using UnityEngine;

namespace SurfaceSystem.UI.MaterialFilter
{
    public class MaterialFilterController : MonoBehaviour
    {
        [SerializeField] private MaterialFilterView _view;

        private MaterialFilterModel _model;

        private void Awake()
        {
            _model = new MaterialFilterModel();            
        }

        private void Start()
        {
            _view.Toggle.onValueChanged.AddListener(OnToggleValueChanged);

            _view.SetToggleValue(_model.ShowAllMaterials, true);
            _view.SetLabel(_model.ShowAllMaterials);
        }

        private void OnToggleValueChanged(bool isOn)
        {
            _model.SetShowAllMaterials(isOn);
            _view.SetLabel(isOn);
            SurfaceEvents.RaiseMaterialFilterChanged(isOn);
        }

        private void OnDestroy()
        {
            _view.Toggle.onValueChanged.RemoveAllListeners();
        }
    }
}