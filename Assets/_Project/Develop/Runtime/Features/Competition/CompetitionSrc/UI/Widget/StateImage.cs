using System;
using AYellowpaper.SerializedCollections;
using UnityEngine.UI;

namespace Features.Competition
{
    [Serializable]
    public class StateImage
    {
        public Image target;
        public SerializedDictionary<WidgetState, ColoredSprite> sprites;
    }
}