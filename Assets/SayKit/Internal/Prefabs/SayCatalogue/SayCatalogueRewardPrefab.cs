using System;
using SayKitInternal;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ParameterHidesMember
// ReSharper disable CompareOfFloatsByEqualityOperator

#endregion

public class SayCatalogueRewardPrefab : MonoBehaviour
{
    [SerializeField] private GameObject defaultSayCatalogue;
    [SerializeField] private GameObject rewardSayCatalogue;
    [SerializeField] private Button sayCatalogueButtonDefault;
    [SerializeField] private Button sayCatalogueButtonReward;
    [SerializeField] private Image rewardImage;
    [SerializeField] private Sprite rewardSprite;
    [SerializeField] private Text rewardLabel;
    [SerializeField] private string rewardText;

    #region Const

    private const string SAYCATALOGUE_REWARD = "SAYCATALOGUE_REWARD_UNITY";
    private const string SOURCE_TYPE_BUTTON_REWARD = "button_reward";
    private const string SOURCE_TYPE_BUTTON = "button";
    private const string EVENT_CLICK = "sk_catalogue_click";
    private const string EVENT_OFFER = "sk_catalogue_offer";
    private const string EVENT_REWARD = "sk_catalogue_reward";

    #endregion
    
    public static Action<string> CatalogueRewardUser;
    
    private void OnEnable()
    {
        UpdatePrefabState();
        CatalogueRewardUser += RewardUser;
    }

    private void OnDisable()
    {
        CatalogueRewardUser -= RewardUser;
    }

    private void Start()
    {
        if (sayCatalogueButtonDefault)
        {
            sayCatalogueButtonDefault.onClick.AddListener(OpenSayCatalogue);
        }

        if (sayCatalogueButtonReward)
        {
            sayCatalogueButtonReward.onClick.AddListener(OpenSayCatalogue);
        }

        if (rewardImage)
        {
            if (rewardSprite != null)
            {
                rewardImage.sprite = rewardSprite;
            }
        }

        if (rewardLabel)
        {
            rewardLabel.text = "+" + rewardText;
        }
    }

    private void OpenSayCatalogue()
    {
        var sourceType = GetSayCatalogueReward() == 1 ? SOURCE_TYPE_BUTTON : SOURCE_TYPE_BUTTON_REWARD;
        SKBridgeManager.Instance.ShowSayCatalogue(sourceType);
        SKBridgeManager.Instance.TrackEvent(name: EVENT_CLICK, extra1: sourceType);
    }

    private static int GetSayCatalogueReward()
    {
        return PlayerPrefs.GetInt(SAYCATALOGUE_REWARD, 0);
    }

    private void UpdatePrefabState()
    {
        if (GetSayCatalogueReward() == 1)
        {
            SKBridgeManager.Instance.TrackSayCatalogueOffer(SOURCE_TYPE_BUTTON);
            SKBridgeManager.Instance.TrackEvent(name: EVENT_OFFER, extra1: SOURCE_TYPE_BUTTON);

            if (defaultSayCatalogue)
            {
                defaultSayCatalogue.SetActive(true);
            }

            if (rewardSayCatalogue)
            {
                rewardSayCatalogue.SetActive(false);
            }
        }
        else
        {
            SKBridgeManager.Instance.TrackSayCatalogueOffer(SOURCE_TYPE_BUTTON_REWARD);
            SKBridgeManager.Instance.TrackEvent(name: EVENT_OFFER, extra1: SOURCE_TYPE_BUTTON_REWARD);

            if (defaultSayCatalogue)
            {
                defaultSayCatalogue.SetActive(false);
            }

            if (rewardSayCatalogue)
            {
                rewardSayCatalogue.SetActive(true);
            }
        }
    }

    private void RewardUser(string placement)
    {
        if (!placement.Equals(SOURCE_TYPE_BUTTON_REWARD))
        {
            return;
        }
        
        if (GetSayCatalogueReward() == 1)
        {
            return;
        }

        if (GetSayCatalogueReward() == 0)
        {
            PlayerPrefs.SetInt(SAYCATALOGUE_REWARD, 1);
            PlayerPrefs.Save();
        
            SKBridgeManager.Instance.TrackEvent(name: EVENT_REWARD);
            SKManager.Instance.Config.sayCatalogueRewarded?.Invoke(Convert.ToInt32(rewardText));
            
            UpdatePrefabState();
        }
    }
}