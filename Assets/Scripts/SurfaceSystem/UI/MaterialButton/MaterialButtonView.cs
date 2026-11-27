using UnityEngine;
using TMPro;

namespace SurfaceSystem.UI.MaterialButton
{
    public class MaterialButtonView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _materialNameText;

        public void UpdateName(string name)
        {
            if (_materialNameText != null)
                _materialNameText.text = name;
        }
    }
}