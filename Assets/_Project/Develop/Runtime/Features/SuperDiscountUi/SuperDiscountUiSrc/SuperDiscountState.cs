using Features.Skin;
using Newtonsoft.Json;
using System;

namespace Features.SuperDiscountUi
{
    [Serializable]
    public class SuperDiscountState
    {
        [JsonProperty("s")] public SuperDiscountSkinType superDiscountSkinType = SuperDiscountSkinType.SkinType_1;
    }
}