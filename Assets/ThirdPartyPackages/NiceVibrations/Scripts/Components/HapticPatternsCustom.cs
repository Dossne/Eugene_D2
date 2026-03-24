using System;
using System.Globalization;
using UnityEngine;

namespace Lofelt.NiceVibrations
{
    /// <summary>
    /// Custom class for HapticPatterns
    /// </summary>
    public static class HapticPatternsCustom
    {
        private static String constantTemplate;
        private static NumberFormatInfo numberFormat;


        public static void Init()
        {
            constantTemplate = (Resources.Load("nv-constant-template") as TextAsset).text;

            numberFormat = new NumberFormatInfo
            {
                NumberDecimalSeparator = "."
            };
        }


        /// <summary>
        /// Do same as Lofelt.NiceVibrations.HapticPatterns.PlayConstant(float amplitude, float frequency, float duration), but not play (only prepare data)
        /// </summary>
        public static (byte[] bytes, GamepadRumble gamepadRumble, float clampedAmplitude, float clampedFrequency) PrepareConstant(float amplitude, float frequency, float duration)
        {
            float clampedAmplitude = Mathf.Clamp(amplitude, 0.0f, 1.0f);
            float clampedFrequency = Mathf.Clamp(frequency, 0.0f, 1.0f);
            float clampedDurationSecs = Mathf.Max(duration, 0.0f);

            string json = constantTemplate
               .Replace("{duration}", clampedDurationSecs.ToString(numberFormat));

            // This preprocessor section will only run for non-mobile platforms
            GamepadRumble rumble = new GamepadRumble();

#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            int rumbleDurationMs = (int)(clampedDurationSecs * 1000);
            const int rumbleEntryDurationMs = 16; // One rumble entry per frame at 60 FPS, which is the limit of what GamepadRumbler can play
            int rumbleEntryCount = rumbleDurationMs / rumbleEntryDurationMs;
            rumble.durationsMs = new int[rumbleEntryCount];
            rumble.lowFrequencyMotorSpeeds = new float[rumbleEntryCount];
            rumble.highFrequencyMotorSpeeds = new float[rumbleEntryCount];

            // Create many rumble entries instead of just one. With just one entry, changing
            // clipLevel while the rumble is playing would have no effect, as GamepadRumbler applies
            // a change only to the next rumble entry, not the one currently playing.
            for (int i = 0; i < rumbleEntryCount; i++)
            {
                rumble.durationsMs[i] = rumbleEntryDurationMs;
                rumble.lowFrequencyMotorSpeeds[i] = 1.0f;
                rumble.highFrequencyMotorSpeeds[i] = 1.0f;
            }
#endif

            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            return (bytes, rumble, clampedAmplitude, clampedFrequency);
        }
    }
}