using UnityEngine;
using UnityEngine.UI;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ParameterHidesMember
// ReSharper disable CompareOfFloatsByEqualityOperator

#endregion

namespace SayKitInternal
{
    public class UISayKitVersionItem : MonoBehaviour
    {
        public Text titleText;
        public Text descriptionText;
        public Button selectButton;
        
        private bool _initialized;

        private void Start()
        {
            if (!_initialized)
            {
                _initialized = true;
                selectButton.onClick.AddListener(OnSelectButtonClicked);
            }
        }

        private void OnSelectButtonClicked()
        {
            UISayKitDebugMenu.GetInstance().SelectNewConfigVersion(titleText.text);
        }
    }
}