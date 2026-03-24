using System;
using Infrastructure.Utilities;
using Newtonsoft.Json;

namespace Features.SeasonPass
{
    [Serializable]
    public class SlotClaimState
    {
        [JsonProperty("i")] public int idx;
        [JsonProperty("s")] public byte state; //max SlotViewType = 7 for byte
        public SlotClaimState(int idx)
        {
            this.idx = idx;
        }

        public bool IsClaimed(SlotViewType slotViewType)
        {
            return BitUtils.TestBit(state, (int)slotViewType);
        }

        public void SetClaimed(SlotViewType slotViewType)
        {
            state = (byte)BitUtils.SetBit(state, (int)slotViewType, true);
        }
    }
}