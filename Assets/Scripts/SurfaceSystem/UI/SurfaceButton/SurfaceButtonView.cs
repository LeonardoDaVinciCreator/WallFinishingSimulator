using UnityEngine;
using TMPro;

namespace SurfaceSystem.UI.SurfaceButton
{
    public class SurfaceButtonView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _surfaceNameText;

        public void UpdateName(string name)
        {
            if (_surfaceNameText != null)
                _surfaceNameText.text = name;
        }
    }
}