using System.Collections.Generic;

namespace Features.SeasonPass
{
    public struct ScrollElementDataDto
    {
        public List<SlotViewData> datas;

        public ScrollElementDataDto(List<SlotViewData> datas)
        {
            this.datas = datas;
        }
    }
}