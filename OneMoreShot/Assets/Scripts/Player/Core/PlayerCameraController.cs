using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils.Input;
using Utils.Misc;
using World;

namespace Player.Core
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform _cinemachineFollowTarget;

        [Header("Camera Data")]
        [SerializeField] private float _cameraRotationSpeed;
        [SerializeField] private float _minCameraAngle;
        [SerializeField] private float _maxCameraAngle;

        [Header("Input Sensitivity Multiplier")]
        [SerializeField] private float _kbmSensitivity;
        [SerializeField] private float _gamepadSensitivity;

        // Mouse Input
        private Vector2 _mouseInput;

        #region Unity Functions

        private void Start()
        {
            _mouseInput = Vector2.zero;

            WorldTimeManager.Instance.OnWorldCustomFixedUpdate += PlayerCameraFixedUpdate;

            CustomInputManager.Instance.PlayerInput.Look.started += HandleMouseInput;
            CustomInputManager.Instance.PlayerInput.Look.performed += HandleMouseInput;
            CustomInputManager.Instance.PlayerInput.Look.canceled += HandleMouseInput;
        }

        private void OnDestroy()
        {
            WorldTimeManager.Instance.OnWorldCustomFixedUpdate -= PlayerCameraFixedUpdate;

            CustomInputManager.Instance.PlayerInput.Look.started -= HandleMouseInput;
            CustomInputManager.Instance.PlayerInput.Look.performed -= HandleMouseInput;
            CustomInputManager.Instance.PlayerInput.Look.canceled -= HandleMouseInput;
        }

        private void Update() => UpdateMouseInput();

        private void PlayerCameraFixedUpdate(float fixedUpdateTime) => UpdateCameraControl();

        #endregion

        #region Camera Control

        private void UpdateCameraControl()
        {
            var sensitivity = CustomInputManager.Instance.LastUsedDeviceInputType == InputType.GamePad ? _gamepadSensitivity : _kbmSensitivity;
            var rotationSpeed = sensitivity * _cameraRotationSpeed;

            var cameraRotation = _cinemachineFollowTarget.rotation.eulerAngles;
            cameraRotation.y += _mouseInput.x * rotationSpeed * WorldTimeManager.Instance.FixedUpdateTime;
            cameraRotation.x += -_mouseInput.y * rotationSpeed * WorldTimeManager.Instance.FixedUpdateTime;
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
            _cinemachineFollowTarget.localRotation = Quaternion.Euler(cameraRotation.x, 0, 0);
        }

        #endregion

        #region Inputs

        private void UpdateMouseInput()
        {
            _mouseInput = CustomInputManager.Instance.PlayerInput.Look.ReadValue<Vector2>();

            // Cuz Unity is an idiot
            _mouseInput.x *= 0.5f;
            _mouseInput.y *= 0.1f;
        }

        private void HandleMouseInput(InputAction.CallbackContext context)
        {
            var path = context.action.activeControl.path;
            var deviceName = context.action.activeControl.displayName;
            CustomInputManager.Instance.UpdateLastUsedDeviceInput(deviceName, path);
        }

        #endregion
    }
}