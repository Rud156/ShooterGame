#region

using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils.Input;
using Utils.Misc;
using World;

#endregion

namespace Player.Base
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform _cinemachineFollowTarget;

        [Header("Camera Data")]
        [SerializeField] private float _cameraRotationSpeed;
        [SerializeField] private float _minCameraAngle;
        [SerializeField] private float _maxCameraAngle;

        [Header("Shoulder Change")]
        [SerializeField] private float _shoulderChangeLerpRate;
        [SerializeField] private float _xOffsetAmount;

        [Header("Input Sensitivity Multiplier")]
        [SerializeField] private float _kbmSensitivity;
        [SerializeField] private float _gamepadSensitivity;

        // Input
        private Vector2 _mouseInput;

        // Shoulder Switch
        private float _currentShoulderLerp;
        private float _startShoulderValue;
        private float _targetShoulderValue;

        #region Unity Functions

        private void Start()
        {
            var cinemachineController = GameObject.FindGameObjectWithTag(TagManager.PlayerCinemachineController);
            var cinemachineVirtualCamera = cinemachineController.GetComponent<CinemachineVirtualCamera>();
            cinemachineVirtualCamera.Follow = _cinemachineFollowTarget;
            cinemachineVirtualCamera.LookAt = _cinemachineFollowTarget;

            _mouseInput = new Vector2();
            _cinemachineFollowTarget.transform.localPosition = new Vector3(-_xOffsetAmount, 0, 0);
            _currentShoulderLerp = 1;

            WorldTimeManager.Instance.OnWorldCustomFixedUpdate += PlayerCameraFixedUpdate;

            CustomInputManager.Instance.PlayerInput.Look.started += HandleMouseInput;
            CustomInputManager.Instance.PlayerInput.Look.performed += HandleMouseInput;
            CustomInputManager.Instance.PlayerInput.Look.canceled += HandleMouseInput;

            CustomInputManager.Instance.PlayerInput.CameraLeftShoulder.started += HandleCameraLeftShoulderSwap;
            CustomInputManager.Instance.PlayerInput.CameraRightShoulder.started += HandleCameraRightShouldSwap;
        }

        private void OnDestroy()
        {
            WorldTimeManager.Instance.OnWorldCustomFixedUpdate -= PlayerCameraFixedUpdate;

            CustomInputManager.Instance.PlayerInput.Look.started -= HandleMouseInput;
            CustomInputManager.Instance.PlayerInput.Look.performed -= HandleMouseInput;
            CustomInputManager.Instance.PlayerInput.Look.canceled -= HandleMouseInput;

            CustomInputManager.Instance.PlayerInput.CameraLeftShoulder.started -= HandleCameraLeftShoulderSwap;
            CustomInputManager.Instance.PlayerInput.CameraRightShoulder.started -= HandleCameraRightShouldSwap;
        }

        private void Update()
        {
            UpdateMouseInput();
            UpdateCameraShoulderLerp();
        }

        #endregion Unity Functions

        #region Utils

        private void PlayerCameraFixedUpdate(float fixedUpdateTime) => UpdateCameraControl();

        #endregion Utils

        #region Camera Control

        private void UpdateCameraShoulderLerp()
        {
            if (_currentShoulderLerp >= 1)
            {
                return;
            }

            var mappedXPosition = Mathf.Lerp(_startShoulderValue, _targetShoulderValue, _currentShoulderLerp);
            _cinemachineFollowTarget.transform.localPosition = new Vector3(mappedXPosition, 0, 0);
            _currentShoulderLerp += _shoulderChangeLerpRate * Time.deltaTime;

            if (_currentShoulderLerp >= 1)
            {
                _cinemachineFollowTarget.transform.localPosition = new Vector3(_targetShoulderValue, 0, 0);
            }
        }

        private void UpdateCameraControl()
        {
            var sensitivity = CustomInputManager.Instance.LastUsedDeviceInputType == CustomInputManager.GamepadGroupString ? _gamepadSensitivity : _kbmSensitivity;
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

        private void HandleCameraLeftShoulderSwap(InputAction.CallbackContext context)
        {
            if (!context.started)
            {
                return;
            }

            _startShoulderValue = _cinemachineFollowTarget.transform.localPosition.x;
            _targetShoulderValue = -_xOffsetAmount;
            _currentShoulderLerp = 0;
        }

        private void HandleCameraRightShouldSwap(InputAction.CallbackContext context)
        {
            if (!context.started)
            {
                return;
            }

            _startShoulderValue = _cinemachineFollowTarget.transform.localPosition.x;
            _targetShoulderValue = _xOffsetAmount;
            _currentShoulderLerp = 0;
        }

        public Vector2 GetMouseInput() => _mouseInput;

        #endregion New Input System
    }
}