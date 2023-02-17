#region

using UI.Player;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Utils.Input
{
    public class InputManager : MonoBehaviour
    {
        private const string GamepadDisplayName = "Controller";
        private const string KeyboardDisplayName = "Keyboard";
        private const string MouseDisplayName = "Mouse";

        private const string GamepadGroupString = "Gamepad";
        private const string KeyboardMouseGroupString = "Keyboard&Mouse";

        private InputMaster _inputMaster;
        private string _lastInputType;

        public delegate void LastInputChanged(string newInputType);
        public event LastInputChanged OnLastInputChanged;

        #region Unity Functions

        private void Start()
        {
            InputSystem.onDeviceChange += HandleDeviceChanged;
            CheckAndUpdateConnectedInputs();
        }

        private void OnDestroy() => InputSystem.onDeviceChange -= HandleDeviceChanged;

        private void HandleDeviceChanged(InputDevice arg1, InputDeviceChange arg2) => CheckAndUpdateConnectedInputs();

        #endregion Unity Functions

        #region Input Setup

        private void SetupInput() => _inputMaster = new InputMaster();

        #endregion Input Setup

        #region External Functions

        public InputMaster InputMaster => _inputMaster;

        public InputMaster.PlayerActions PlayerInput => _inputMaster.Player;

        public string LastInputType => _lastInputType;

        public void EnablePlayerControls() => _inputMaster.Player.Enable();

        public void DisablePlayerControls() => _inputMaster.Player.Disable();

        #endregion External Functions

        #region Utils

        private void CheckAndUpdateConnectedInputs()
        {
            var devices = InputSystem.devices;
            var lastUpdateTime = double.MinValue;
            var lastInputName = _lastInputType;

            foreach (var device in devices)
            {
                var deviceUpdateTime = device.lastUpdateTime;
                var deviceName = device.displayName;
                if (deviceUpdateTime > lastUpdateTime)
                {
                    lastUpdateTime = deviceUpdateTime;
                    lastInputName = deviceName.Contains(GamepadDisplayName) ? GamepadGroupString : KeyboardMouseGroupString;
                }
            }

            _lastInputType = lastInputName;
            OnLastInputChanged?.Invoke(lastInputName);

            // TODO: Check for a better way to detect the Last Input Type
            HUD_PlayerAbilityDisplay.Instance.UpdateAbilityTriggers(lastInputName);
        }

        #endregion Utils

        #region Singleton

        public static InputManager Instance { get; private set; }

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