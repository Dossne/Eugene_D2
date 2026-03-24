using UnityEngine;

using Infrastructure.Utilities;
using System;
using System.Collections.Generic;

namespace Features.BottomPanel
{
    public class BottomPanelBase : MonoBehaviour
    {
        [SerializeField] private RectTransform buttonParent;
        [SerializeField] private BottomPanelButton buttonPf;
        private Dictionary<string, BottomPanelButton> buttons = new();

        public void SetObjectActive(bool isEnabled)
        {
            gameObject.SetObjectActive(isEnabled);
        }

        public void SetButtonNotifierActive(string buttonId, bool isActive) 
        {
            if (buttons.TryGetValue(buttonId, out var button))
                button.SetNotifierActive(isActive);
        }

        public void CreateButton(BottomPanelButtonData bottomPanelButtonData, Sprite icon, Action callback)
        {
            if (buttons.ContainsKey(bottomPanelButtonData.id))
                return;
            
            buttons.Add(bottomPanelButtonData.id, Instantiate(buttonPf, buttonParent));
            buttons[bottomPanelButtonData.id].Construct(bottomPanelButtonData, icon, callback);
        }

        public void Clear()
        {
            foreach (var item in buttons)
            {
                item.Value.StopAnimation();
                Destroy(item.Value.gameObject);
            }                
            buttons.Clear();    
        }

        public void ToggleButton(string id) 
        {
            foreach (var item in buttons)
                item.Value.SetToggled(item.Key == id);
        }

        public bool IsToggled(string id) 
        {
            if (!buttons.TryGetValue(id, out var button)) 
                return false;

            return button.IsToggled;
        }
    }
}