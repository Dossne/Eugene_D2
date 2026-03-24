using Features.ScrollList;
using System;

namespace Features.PlayerProfile
{
    public class PlayerProfileAvatarList : ScrollElementList<(string id, bool isSelected, bool isSaved)>
    {
        private PlayerProfileAvatarListElement selectedElement = null;
        private PlayerProfileAvatarListElement savedElement = null;

        public void SelectAvatar(string avatarId)
        {
            if (selectedElement != null)
                selectedElement.SetSelected(false);

            var element = Elements.Find(x => x.ElementDatasource.id == avatarId);
            if (element != null && element is PlayerProfileAvatarListElement avatarElement)
            {
                selectedElement = avatarElement;
                selectedElement.SetSelected(true);
            }            
        }

        public void SaveAvatar(string avatarId)
        {
            if (savedElement != null)
                savedElement.SetSaved(false);

            var element = Elements.Find(x => x.ElementDatasource.id == avatarId);
            if (element != null && element is PlayerProfileAvatarListElement avatarElement)
            {
                savedElement = avatarElement;
                savedElement.SetSaved(true);
            }
        }
    }
}