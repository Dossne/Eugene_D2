using Features.ScrollList;
using Infrastructure.Utilities;
using UnityEngine;
using UnityEngine.UI;


namespace Features.PlayerProfile
{
    public class PlayerProfileAvatarListElement : ScrollElement<(string id, bool isSelected, bool isSaved)>
    {
        [SerializeField] private Image  avatar;
        [SerializeField] private Image  selectionFrame;
        [SerializeField] private Image  savedMark;
        [SerializeField] private Button button;

        public void SetSelected(bool isSelected)
        {
            datasource.isSelected = isSelected;
            selectionFrame.gameObject.SetObjectActive(datasource.isSelected);
        }

        public void SetSaved(bool isSaved) 
        {
            datasource.isSaved = isSaved;
            savedMark.gameObject.SetObjectActive(datasource.isSaved);
        }

        protected override void InitializeImpl()
        {
            avatar.sprite = spriteAtlasService.GetFromMain(datasource.id);
            selectionFrame.gameObject.SetObjectActive(datasource.isSelected);
            savedMark.gameObject.SetObjectActive(datasource.isSaved);
            button.onClick.AddListener(HandleClick);
        }

        protected override void DeinitializeImpl()
        {
            button.onClick.RemoveListener(HandleClick);
        }    
    }
}