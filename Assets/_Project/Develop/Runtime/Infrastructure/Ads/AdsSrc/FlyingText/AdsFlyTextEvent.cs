using R3;
using UnityEngine;

namespace Infrastructure.Ads
{
    public class AdsFlyTextEvent : ReactiveCommand
    {
        public string localKey;
        public Vector3 position;


        public void Request(string localKey, Vector3 position)
        {
            this.localKey = localKey;
            this.position = position;
            Execute(Unit.Default);
        }
    }
}