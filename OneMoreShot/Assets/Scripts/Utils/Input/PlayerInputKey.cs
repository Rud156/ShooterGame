using UnityEngine.InputSystem;

namespace Utils.Input
{
    public struct PlayerInputKey
    {
        public bool KeyPressedThisFrame;
        public bool KeyReleasedThisFrame;
        public bool KeyPressed;

        public void UpdateInputData(InputAction.CallbackContext context)
        {
            var keyPressedThisFrame = context.started;
            var keyReleasedThisFrame = context.canceled;

            if (keyPressedThisFrame && !KeyPressedThisFrame)
            {
                KeyPressedThisFrame = true;
                KeyPressed = true;
            }

            if (keyReleasedThisFrame && !KeyReleasedThisFrame)
            {
                KeyReleasedThisFrame = true;
                KeyPressed = false;
            }

            var path = context.action.activeControl.device.path;
            var deviceName = context.action.activeControl.device.displayName;
            CustomInputManager.Instance.UpdateLastUsedDeviceInput(deviceName, path);
        }

        public void ResetPerFrameInput()
        {
            KeyPressedThisFrame = false;
            KeyReleasedThisFrame = false;
        }
    }
}