using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Boosters.Model;
using Features.Character;
using Infrastructure.AssetManagement;
using UnityEngine;

namespace Features.Boosters.Behaviour.InGameBooster
{
    public sealed class SuperMagnetBehaviour : InGameBoosterBehaviour
    {
        private readonly CharacterManager characterManager;
        private readonly Instantiator instantiator;
        private CancellationTokenSource cts;
        private MagnetField field;
        private bool isActive;

        public SuperMagnetBehaviour(InGameBoosterModelBase model, InGameBoosterSlotView view, CharacterManager characterManager, Instantiator instantiator) :
            base(model, view)
        {
            this.characterManager = characterManager;
            this.instantiator = instantiator;
            this.cts = new CancellationTokenSource();
        }


        protected override void ActionsOnApply()
        {
            isActive = true;
            ApplyBoosterAsync().Forget();
        }


        protected override void ActionsOnUnapply()
        {
            if (field != null)
                field.Deactivate();

            isActive = false;
        }


        public override void Deinitialize()
        {
            cts.Cancel();
            cts.Dispose();

            ActionsOnUnapply();
        }


        private async UniTaskVoid ApplyBoosterAsync()
        {
            if (field == null)
            {
                field = await instantiator.InstantiateAsync<MagnetField>(AssetKeys.MagnetField,
                                                                         isInstantiateAsync: false,
                                                                         cancellationToken: cts.Token);
                ParentToCharacter(field);
            }

            if(isActive)
                field.Activate();
        }


        private void ParentToCharacter(MagnetField field)
        {
            var viewRoot = characterManager.GetViewRoot();

            field.transform.SetParent(viewRoot);
            field.transform.localPosition = Vector3.zero;
            field.transform.localRotation = Quaternion.identity;
            field.transform.localScale = Vector3.one;
        }
    }
}