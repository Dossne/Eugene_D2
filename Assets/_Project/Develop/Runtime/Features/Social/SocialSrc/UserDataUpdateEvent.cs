using Nakama;
using R3;
using System.Collections.Generic;


namespace Features.Social
{
    public class UserDataUpdateEvent : ReactiveCommand
    {
        public string DisplayName { get; private set; }
        public PlayerMetaData PlayerMetaData { get; private set; }



        public void Execute(string displayName, PlayerMetaData playerMetaData)
        {
            DisplayName = displayName;
            PlayerMetaData = playerMetaData;
            base.Execute(Unit.Default);
        }
    }
}