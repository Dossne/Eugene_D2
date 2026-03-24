using System;
using System.Collections.Generic;
using Infrastructure.SpriteAtlasControl;
using UnityEngine;
using VContainer;

namespace Features.Boosters
{
    public class BoostersPanelHud : MonoBehaviour
    {
        public event Action<BoosterType> OnSlotClick;

        [SerializeField] private InGameBoosterSlotView boosterViewPf;
        [SerializeField] private Transform slotRoot;
        private readonly Dictionary<BoosterType, InGameBoosterSlotView> boosterViews = new();
        private List<BoosterData> startBoosterConfig;
        private SpriteAtlasService spriteAtlasService;
        private bool isInit;


        [Inject]
        public void Construct(SpriteAtlasService spriteAtlasService)
        {
            this.spriteAtlasService = spriteAtlasService;
        }


        public void Construct(List<BoosterData> startBoosterConfig)
        {
            this.startBoosterConfig = startBoosterConfig;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            foreach (var configData in startBoosterConfig)
            {
                var view = Instantiate(boosterViewPf, slotRoot);
                view.Construct(configData.type, spriteAtlasService.GetFromMain(configData.iconName));
                view.Initialize();
                view.OnClick += BoosterView_OnClick;
                boosterViews.Add(configData.type, view);
            }

            SetObjectActive(true);
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            SetObjectActive(false);

            foreach (var view in boosterViews.Values)
            {
                view.OnClick -= BoosterView_OnClick;
                view.Deinitialize();
                view.Destroy();
            }

            boosterViews.Clear();
            OnSlotClick = null;

            isInit = false;
        }


        public void RefreshSlotState(BoosterType type, BoosterStateType stateType, string count, string lockText, string freeText)
        {
            boosterViews[type].RefreshState(stateType, count, lockText, freeText);
        }


        public InGameBoosterSlotView GetSlotView(BoosterType type)
        {
            return boosterViews[type];
        }

        private void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }


        private void BoosterView_OnClick(BoosterType boosterType)
        {
            OnSlotClick?.Invoke(boosterType);
        }
    }
}