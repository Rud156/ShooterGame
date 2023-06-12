#region

using UnityEngine;
using UnityEngine.InputSystem;

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

        #region Unity Functions

        private void Start()
        {
            PlayerInput.MouseDelta.started += HandleMouseMoved;
            PlayerInput.MouseDelta.performed += HandleMouseMoved;
            PlayerInput.MouseDelta.canceled += HandleMouseMoved;
        }

        private void OnDestroy()
        {
            PlayerInput.MouseDelta.started -= HandleMouseMoved;
            PlayerInput.MouseDelta.performed -= HandleMouseMoved;
            PlayerInput.MouseDelta.canceled -= HandleMouseMoved;
        }

        #endregion Unity Functions

        #region Input Setup

        private void SetupInput() => _inputMaster = new InputMaster();

        #endregion Input Setup

        #region External Functions

        public InputMaster InputMaster => _inputMaster;

        public InputMaster.PlayerActions PlayerInput => _inputMaster.Player;

        public InputMaster.PlayerOldActions PlayerOldInput => _inputMaster.PlayerOld;

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
                if (currentInputType == GamepadGroupString)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }

                _lastUsedDeviceInputType = currentInputType;
                OnLastUsedInputChanged?.Invoke(_lastUsedDeviceInputType);
            }
        }

        #endregion External Functions

        #region Utils

        private void HandleMouseMoved(InputAction.CallbackContext context)
        {
            var mouseInput = _inputMaster.PlayerOld.Look.ReadValue<Vector2>();
            if (mouseInput != Vector2.zero)
            {
                var path = context.action.activeControl.path;
                var deviceName = context.action.activeControl.displayName;
                UpdateLastUsedDeviceInput(deviceName, path);
            }
        }

        #endregion Utils

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