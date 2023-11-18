using UnityEngine.InputSystem;

namespace Utils.Input
{
    public struct PlayerInputKey
    {
        public bool KeyPressedThisFrame;
        public bool KeyReleasedThisFrame;
        public bool KeyPressed;

        private bool _isToggleKey;
        private bool _isToggleActive;

        public void SetKeyToggle(bool toggle)
        {
            _isToggleKey = toggle;
            if (_isToggleKey)
            {
                _isToggleActive = false;
            }
        }

        public void ClearToggleActiveState()
        {
            _isToggleActive = false;
            KeyPressed = false;
        }

        public void UpdateInputData(InputAction.CallbackContext context)
        {
            var keyPressedThisFrame = context.started;
            var keyReleasedThisFrame = context.canceled;

            if (keyPressedThisFrame && !KeyPressedThisFrame)
            {
                KeyPressedThisFrame = true;
            }

            if (keyReleasedThisFrame && !KeyReleasedThisFrame)
            {
                KeyReleasedThisFrame = true;
            }

            if (_isToggleKey && context.started)
            {
                if (!_isToggleActive)
                {
                    KeyPressed = true;
                    _isToggleActive = true;
                }
                else
                {
                    KeyPressed = false;
                    _isToggleActive = false;
                }
            }
            else if (!_isToggleKey)
            {
                KeyPressed = context.performed;
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