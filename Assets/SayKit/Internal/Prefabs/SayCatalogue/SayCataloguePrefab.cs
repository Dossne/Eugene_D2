using SayKitInternal;
using UnityEngine;
using Button = UnityEngine.UI.Button;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ParameterHidesMember
// ReSharper disable CompareOfFloatsByEqualityOperator

#endregion

public class SayCataloguePrefab : MonoBehaviour
{
    [SerializeField] private Button sayCatalogueButton;

    private void OnEnable()
    {
        SKBridgeManager.Instance.TrackSayCatalogueOffer("button");
    }

    private void Start()
    {
        if (sayCatalogueButton != null)
        {
            sayCatalogueButton.onClick.AddListener(OpenSayCatalogue);
        }
    }

    private void OpenSayCatalogue()
    {
        SKBridgeManager.Instance.ShowSayCatalogue("button");
    }
}