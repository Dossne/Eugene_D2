using Features.ScrollList;
using Infrastructure.PurchaseSystem;
using System.Collections.Generic;

namespace Features.ShopUi
{
    public class ShopOfferScrollElementList : ScrollElementList<PurchaseOfferUiDatasource>
    {
        public bool HasOffer(OfferId inAppOfferId)
        {
            return elements.Exists(x => x.ElementDatasource.id == inAppOfferId);
        }

        public ScrollElement<PurchaseOfferUiDatasource> GetScrollElement(OfferId inAppOfferId) 
        {
            return elements.Find(x => x.ElementDatasource.id == inAppOfferId);
        }

        public void RemoveOffersIfNotExist(List<OfferId> inAppOfferIds)
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                if (inAppOfferIds.Exists(x => x == elements[i].ElementDatasource.id))
                    continue;
                RemoveElement(elements[i]);
            }
        }

        public void ScrollToOfferType(PurchaseOfferUiType purchaseOfferType, bool isAnimated) 
        {
            ScrollToElement(elements.FindIndex(x => x.ElementDatasource.purchaseOfferUiType == purchaseOfferType), isAnimated);
        }
    }
}