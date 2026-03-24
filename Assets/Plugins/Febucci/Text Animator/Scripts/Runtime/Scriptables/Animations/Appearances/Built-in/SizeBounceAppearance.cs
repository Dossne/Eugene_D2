using Febucci.UI.Core;
using Febucci.UI.Effects;
using UnityEngine;
namespace Febucci.UI.Effects
{
    [UnityEngine.Scripting.Preserve]
    [CreateAssetMenu(fileName = "Size Bounce Appearance", menuName = "Text Animator/Animations/Appearances/Size Bounce Appearance")]
    [EffectInfo("size_bounce", EffectCategory.Appearances)]
    public sealed class SizeBounceAppearance : AppearanceScriptableBase
    {
        float amplitude;
        public float baseAmplitude = 2;

        public override void ResetContext(TAnimCore animator)
        {
            base.ResetContext(animator);
            amplitude = baseAmplitude * -1 + 1;
        }

        public override void ApplyEffectTo(ref Core.CharacterData character, TAnimCore animator)
        {
            character.current.positions.LerpUnclamped(
                character.current.positions.GetMiddlePos(),
                character.passedTime < duration * 0.5 ? (Tween.EaseOut(character.passedTime / (duration * 0.5f)) * amplitude) 
                                                      : (Tween.EaseIn(2 - character.passedTime / (duration * 0.5f)) * amplitude)
                );
        }

        public override void SetModifier(ModifierInfo modifier)
        {
            switch (modifier.name)
            {
                case "a": amplitude = baseAmplitude * modifier.value; break;
                default: base.SetModifier(modifier); break;
            }
        }
    }
}