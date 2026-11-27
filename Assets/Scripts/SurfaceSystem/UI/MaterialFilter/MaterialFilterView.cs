using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SurfaceSystem.UI.MaterialFilter
{
    public class MaterialFilterView: MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private string _labelOn = "Показать все материалы";
        [SerializeField] private string _labelOf = "Оставить только подходящие материалы";

        public Toggle Toggle => _toggle;

        public void SetLabel(bool isShowAll) => _labelText.text = isShowAll ? _labelOn : _labelOf;

        public void SetToggleValue(bool isOn, bool withoutNotify  = true)
        {
            if (withoutNotify)
                _toggle.SetIsOnWithoutNotify(isOn);
            else
                _toggle.isOn = isOn;
        }
    }
}