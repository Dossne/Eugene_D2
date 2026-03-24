using Infrastructure.Configs;
using Infrastructure.Settings;
using UnityEngine;
using VContainer;

namespace Infrastructure.InputControl
{
    public class InputService : MonoBehaviour
    {
        [SerializeField] private Canvas thisCanvas;
        [SerializeField] private UltimateJoystick movementJoystick;
        [SerializeField] private string movementJoystickName;
        [SerializeField] private int canvasSortOrder = -1; // must be under mainCanvas
        
        private bool isEnabled;
        private SystemSettingsConfig systemSettingsConfig;

        public float X => HasInput() ? UltimateJoystick.GetHorizontalAxis(movementJoystickName) : 0;
        public float Y => HasInput() ? UltimateJoystick.GetVerticalAxis(movementJoystickName) : 0;

        [Inject]
        public void Construct(ConfigProvider configProvider)
        {
            this.systemSettingsConfig = configProvider.SystemSettingsConfig;
        }

        public void Initialize()
        {
            movementJoystick.SetJoystickName(movementJoystickName);
            movementJoystick.extendRadius = systemSettingsConfig.SettingsData.haveJoystickFollow;
            movementJoystick.Initialize();
            isEnabled = true;
            thisCanvas.sortingOrder = canvasSortOrder;
        }


        public bool HasInput()
        {
            return UltimateJoystick.GetJoystickState(movementJoystickName);
        }


        public float Distance()
        {
            return UltimateJoystick.GetDistance(movementJoystickName);
        }


        public void EnableInput()
        {
            if (isEnabled)
                return;

            UltimateJoystick.EnableJoystick(movementJoystickName);
            thisCanvas.enabled = true;
            
            isEnabled = true;
        }


        public void DisableInput()
        {
            if (!isEnabled)
                return;

            UltimateJoystick.DisableJoystick(movementJoystickName);
            thisCanvas.enabled = false;
            ResetInput();

            isEnabled = false;
        }


        /// <summary>
        /// Reset and take away joystick from player
        /// </summary>
        public void ResetInput()
        {
            if (!isEnabled)
                return;

            movementJoystick.UpdatePositioning();
        }
    }
}