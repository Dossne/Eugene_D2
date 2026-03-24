using Features.Collectables;
using Features.Events;
#if PR_CHEAT
using Infrastructure.Cheat;
#endif
using UnityEngine;
using VContainer;

namespace Features.Character
{
    public class ItemCollectTrigger : MonoBehaviour
    {
        private bool isInit;

        private CollectItemEvent collectItemEvent;
#if PR_CHEAT
        private CheatService cheatService;
#endif


        [Inject]
        public void Construct(CollectItemEvent collectItemEvent
#if PR_CHEAT
                              , CheatService cheatService
#endif
                             )
        {
            this.collectItemEvent = collectItemEvent;
#if PR_CHEAT
            this.cheatService = cheatService;
#endif
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null)
            {
                return;
            }

            if (!other.attachedRigidbody.TryGetComponent(out CollectableItem item))
                return;
#if PR_CHEAT
            if (item.Group == ItemGroup.Damage && !cheatService.IsBombDamage)
                item.SetItemGroup(ItemGroup.Collectable);
#endif
            collectItemEvent.Execute(item);
        }
    }
}