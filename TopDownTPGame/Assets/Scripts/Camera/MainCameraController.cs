#region

using UnityEngine;
using UnityEngine.InputSystem;
using Utils.Input;
using Utils.Misc;

#endregion

namespace Camera
{
    public class MainCameraController : MonoBehaviour
    {
        [Header("Camera Data")]
        [SerializeField] private Transform _cameraHolder;
        [SerializeField] private float _cameraRotationSpeed;
        [SerializeField] private float _minCameraAngle;
        [SerializeField] private float _maxCameraAngle;

        private Vector2 _mouseInput;

        #region Unity Functions

        private void Start()
        {
            _mouseInput = new Vector2();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            CustomInputManager.Instance.PlayerInput.Look.started += HandleMouseInput;
            CustomInputManager.Instance.PlayerInput.Look.performed += HandleMouseInput;
            CustomInputManager.Instance.PlayerInput.Look.canceled += HandleMouseInput;
        }

        private void OnDestroy()
        {
            CustomInputManager.Instance.PlayerInput.Look.started -= HandleMouseInput;
            CustomInputManager.Instance.PlayerInput.Look.performed -= HandleMouseInput;
            CustomInputManager.Instance.PlayerInput.Look.canceled -= HandleMouseInput;
        }

        private void Update() => UpdateMouseInput();

        private void FixedUpdate() => UpdateCameraControl();

        #endregion Unity Functions

        #region Camera Control

        private void UpdateCameraControl()
        {
            var cameraRotation = _cameraHolder.rotation.eulerAngles;
            cameraRotation.y += _mouseInput.x * _cameraRotationSpeed * Time.fixedDeltaTime;
            cameraRotation.x += -_mouseInput.y * _cameraRotationSpeed * Time.fixedDeltaTime;
            cameraRotation.x = ExtensionFunctions.To360Angle(cameraRotation.x);

            switch (cameraRotation.x)
            {
                // Clamp X Rotation
                case >= 0 and <= 180:
                {
                    if (cameraRotation.x > _maxCameraAngle)
                    {
                        cameraRotation.x = _maxCameraAngle;
                    }

                    break;
                }
                case > 180 and <= 360:
                {
                    var negatedAngle = cameraRotation.x - 360;
                    if (negatedAngle < _minCameraAngle)
                    {
                        cameraRotation.x = _minCameraAngle;
                    }

                    break;
                }
            }

            transform.rotation = Quaternion.Euler(0, cameraRotation.y, 0);
            _cameraHolder.localRotation = Quaternion.Euler(cameraRotation.x, 0, 0);
        }

        #endregion Camera Control

        #region New Input System

        private void UpdateMouseInput()
        {
            _mouseInput = CustomInputManager.Instance.PlayerInput.Look.ReadValue<Vector2>();

            // Cuz Unity is an idiot
            _mouseInput *= 0.5f;
            _mouseInput *= 0.1f;
        }

        private void HandleMouseInput(InputAction.CallbackContext context)
        {
            var path = context.action.activeControl.path;
            var deviceName = context.action.activeControl.displayName;
            CustomInputManager.Instance.UpdateLastUsedDeviceInput(deviceName, path);
        }

        public Vector2 GetMouseInput() => _mouseInput;

        #endregion New Input System
    }
}