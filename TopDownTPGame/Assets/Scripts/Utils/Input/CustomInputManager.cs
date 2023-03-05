#region

using UnityEngine;

#endregion

namespace Utils.Input
{
    public class CustomInputManager : MonoBehaviour
    {
        private const string GamepadDisplayName = "Controller";
        private const string KeyboardDisplayName = "Keyboard";
        private const string MouseDisplayName = "Mouse";

        public const string GamepadGroupString = "Gamepad";
        public const string KeyboardMouseGroupString = "Keyboard&Mouse";

        public delegate void LastUsedInputChanged(string currentInputGroup);
        public event LastUsedInputChanged OnLastUsedInputChanged;

        private InputMaster _inputMaster;
        private string _lastUsedDeviceInputType = KeyboardMouseGroupString;

        #region Input Setup

        private void SetupInput() => _inputMaster = new InputMaster();

        #endregion Input Setup

        #region External Functions

        public InputMaster InputMaster => _inputMaster;

        public InputMaster.PlayerActions PlayerInput => _inputMaster.Player;

        public string LastUsedDeviceInputType => _lastUsedDeviceInputType;

        public void EnablePlayerControls() => _inputMaster.Player.Enable();

        public void DisablePlayerControls() => _inputMaster.Player.Disable();

        public void UpdateLastUsedDeviceInput(string deviceName, string path)
        {
            var hasKeyboard = deviceName.Contains(KeyboardDisplayName) || path.Contains(KeyboardDisplayName);
            var hasMouse = deviceName.Contains(MouseDisplayName) || path.Contains(MouseDisplayName);
            var hasGamepad = deviceName.Contains(GamepadDisplayName) || path.Contains(GamepadDisplayName);

            var currentInputType = hasGamepad ? GamepadGroupString : KeyboardMouseGroupString;
            if (!hasGamepad && !hasKeyboard && !hasMouse)
            {
                Debug.LogWarning("Invalid Input detected. Moving to Last Used");
                currentInputType = _lastUsedDeviceInputType;
            }

            if (currentInputType != _lastUsedDeviceInputType)
            {
                _lastUsedDeviceInputType = currentInputType;
                OnLastUsedInputChanged?.Invoke(_lastUsedDeviceInputType);
            }
        }

        #endregion External Functions

        #region Singleton

        public static CustomInputManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            SetupInput();
        }

        #endregion Singleton
    }
}