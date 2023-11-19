using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils.Input;
using Utils.Misc;
using World;

namespace Player.Core
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private const float MaxTerrainRaycastDistance = 1000;

        [Header("Core Movement")]
        [SerializeField] private float _runningSpeed;
        [SerializeField] private float _jumpVelocity;
        [SerializeField] private float _gravityMultiplier;
        [SerializeField] private float _airMultiplier;
        [SerializeField] private float _groundedVelocityTriggerThreshold;

        [Header("Temp")]
        [SerializeField] private Collider _groundCollider;

        // Player State
        private List<PlayerState> _playerStateStack;
        private float _currentStateVelocity;
        private Vector3 _characterVelocity;
        private bool _jumpReset;
        private bool _isGrounded;
        public bool IsGrounded => _isGrounded;
        public PlayerState TopPlayerState => _playerStateStack[^1];

        // Inputs
        private Vector2 _coreMovementInput;
        private Vector2 _lastNonZeroCoreMovementInput;
        private PlayerInputKey _jumpKey;
        private PlayerInputKey _abilityPrimaryKey;
        private PlayerInputKey _abilitySecondaryKey;

        // Character Controller
        private CharacterController _characterController;

        // Player Rotation
        private Camera _mainCamera;

        // Delegates
        public delegate void PlayerStatePushed(PlayerState newState);
        public delegate void PlayerStatePopped(PlayerState poppedState);
        public delegate void PlayerStateChanged(PlayerState previousState, PlayerState newState);
        public delegate void PlayerGroundedChange(bool previousState, bool newState);
        public delegate void PlayerJumped();
        public event PlayerStatePushed OnPlayerStatePushed;
        public event PlayerStatePopped OnPlayerStatePopped;
        public event PlayerStateChanged OnPlayerStateChanged;
        public event PlayerGroundedChange OnPlayerGroundedChanged;
        public event PlayerJumped OnPlayerJumped;

        #region Unity Functions

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
            _mainCamera = Camera.main;
            WorldTimeManager.Instance.OnWorldCustomFixedUpdate += PlayerFixedUpdate;

            InitializeInputEvents();
            _jumpKey = new PlayerInputKey { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };
            _abilityPrimaryKey = new PlayerInputKey { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };
            _abilitySecondaryKey = new PlayerInputKey { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };

            _playerStateStack = new List<PlayerState>();
            _coreMovementInput = Vector2.zero;
            _lastNonZeroCoreMovementInput = Vector2.zero;
            _characterVelocity = Vector3.zero;
            _currentStateVelocity = 0;
            PushPlayerState(PlayerState.Idle);
        }

        private void OnDestroy()
        {
            DeInitializeInputEvents();
            WorldTimeManager.Instance.OnWorldCustomFixedUpdate -= PlayerFixedUpdate;
        }

        private void Update() => UpdateMovementInput();

        private void PlayerFixedUpdate(float fixedUpdateTime)
        {
            UpdateGroundedState();
            UpdatePlayerMovement();
            ApplyFinalMovement();
            ResetFrameInputs();
        }

        #endregion

        #region Player Movement

        private void UpdateGroundedState()
        {
            var isGrounded = _characterController.isGrounded;
            if (!isGrounded)
            {
                _characterVelocity.y -= Physics.gravity.y * _gravityMultiplier;
            }
            else
            {
                _characterVelocity.y = -Physics.gravity.y;
            }

            // Means that the player is falling down
            if (!isGrounded && _playerStateStack[^1] != PlayerState.Falling)
            {
                PushPlayerState(PlayerState.Falling);
            }

            if (IsGrounded != isGrounded)
            {
                OnPlayerGroundedChanged?.Invoke(IsGrounded, isGrounded);
                _isGrounded = isGrounded;
            }
        }

        private void UpdatePlayerMovement()
        {
            ProcessJumpInput();
            switch (_playerStateStack[^1])
            {
                case PlayerState.Idle:
                    UpdateIdleState();
                    break;

                case PlayerState.Running:
                    UpdateRunningState();
                    break;

                case PlayerState.Falling:
                    UpdateFallingState();
                    break;

                case PlayerState.Dead:
                    UpdateDeadState();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (CustomInputManager.Instance.LastUsedDeviceInputType)
            {
                case InputType.GamePad:
                    UpdatePlayerRotationGamepadRotation();
                    break;

                case InputType.KeyboardMouse:
                    UpdatePlayerRotationKeyboard();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdateCoreMovement();
        }

        #region Core Movement

        private void UpdatePlayerRotationKeyboard()
        {
            var mousePosition = CustomInputManager.Instance.PlayerInput.MousePosition.ReadValue<Vector2>();
            var rayStartPoint = _mainCamera.ScreenPointToRay(mousePosition);
            if (_groundCollider.Raycast(rayStartPoint, out var hitInfo, MaxTerrainRaycastDistance))
            {
                var worldMousePosition = hitInfo.point;
                var direction = worldMousePosition - transform.position;

                var computedRotation = Quaternion.LookRotation(direction).eulerAngles;
                computedRotation.x = 0;
                computedRotation.z = 0;
                transform.eulerAngles = computedRotation;
            }
        }

        private void UpdatePlayerRotationGamepadRotation()
        {
            var gamepadRotation = CustomInputManager.Instance.PlayerInput.Move.ReadValue<Vector2>();
            if (gamepadRotation == Vector2.zero)
            {
                return;
            }

            var angle = Mathf.Atan2(gamepadRotation.x, gamepadRotation.y) * Mathf.Rad2Deg;
            if (angle != 0)
            {
                transform.rotation = Quaternion.Euler(0, angle, 0);
            }
        }

        private void UpdateCoreMovement()
        {
            var characterTransform = transform;
            var forward = characterTransform.forward;

            if (_playerStateStack[^1] != PlayerState.Falling)
            {
                var groundedMovement = forward * _coreMovementInput.y;
                if (CustomInputManager.Instance.LastUsedDeviceInputType == InputType.KeyboardMouse)
                {
                    var right = characterTransform.right;
                    groundedMovement += right * _coreMovementInput.x;
                }

                groundedMovement.y = 0;
                groundedMovement = _currentStateVelocity * groundedMovement.normalized;

                _characterVelocity.x = groundedMovement.x;
                _characterVelocity.z = groundedMovement.z;
            }
            else
            {
                var airMovement = forward * _lastNonZeroCoreMovementInput.y;
                if (CustomInputManager.Instance.LastUsedDeviceInputType == InputType.KeyboardMouse)
                {
                    var right = characterTransform.right;
                    airMovement += right * _lastNonZeroCoreMovementInput.x;
                }

                airMovement.y = 0;
                airMovement = airMovement.normalized * (_airMultiplier * _currentStateVelocity);

                var clampedXVelocity = Mathf.Clamp(_characterVelocity.x + airMovement.x, -_currentStateVelocity, _currentStateVelocity);
                var clampedZVelocity = Mathf.Clamp(_characterVelocity.z + airMovement.z, -_currentStateVelocity, _currentStateVelocity);

                _characterVelocity.x = clampedXVelocity;
                _characterVelocity.z = clampedZVelocity;
            }
        }

        #endregion

        private void ApplyFinalMovement() => _characterController.Move(_characterVelocity * WorldTimeManager.Instance.FixedUpdateTime);

        #region Player State Input Updates

        private void ProcessJumpInput()
        {
            var jumpPressed = _jumpKey.KeyPressedThisFrame;
            if (!jumpPressed || !_jumpReset)
            {
                return;
            }

            _characterVelocity.y = _jumpVelocity;
            _jumpReset = false;
            OnPlayerJumped?.Invoke();
        }

        private void UpdateIdleState()
        {
            _currentStateVelocity = 0;
            if (!HasNoDirectionalInput())
            {
                PushPlayerState(PlayerState.Running);
            }
        }

        private void UpdateRunningState()
        {
            _currentStateVelocity = _runningSpeed;
            if (HasNoDirectionalInput())
            {
                PopPlayerState();
            }
        }

        private void UpdateFallingState()
        {
            if (IsGrounded)
            {
                PopPlayerState();
            }
        }

        private void UpdateDeadState()
        {
            // Do nothing here for now...
            // Will be needed for ReSpawns
        }

        #endregion

        #endregion

        #region Inputs

        private void InitializeInputEvents()
        {
            CustomInputManager.Instance.EnablePlayerControls();

            var playerInputMaster = CustomInputManager.Instance.PlayerInput;

            playerInputMaster.Move.started += HandleMoveInput;
            playerInputMaster.Move.performed += HandleMoveInput;
            playerInputMaster.Move.canceled += HandleMoveInput;

            playerInputMaster.Jump.started += HandlePlayerPressJump;
            playerInputMaster.Jump.performed += HandlePlayerPressJump;
            playerInputMaster.Jump.canceled += HandlePlayerPressJump;

            playerInputMaster.AbilityPrimary.started += HandlePlayerPressAbilityPrimary;
            playerInputMaster.AbilityPrimary.performed += HandlePlayerPressAbilityPrimary;
            playerInputMaster.AbilityPrimary.canceled += HandlePlayerPressAbilityPrimary;

            playerInputMaster.AbilitySecondary.started += HandlePlayerPressAbilitySecondary;
            playerInputMaster.AbilitySecondary.performed += HandlePlayerPressAbilitySecondary;
            playerInputMaster.AbilitySecondary.canceled += HandlePlayerPressAbilitySecondary;
        }

        private void DeInitializeInputEvents()
        {
            var playerInputMaster = CustomInputManager.Instance.PlayerInput;

            playerInputMaster.Move.started -= HandleMoveInput;
            playerInputMaster.Move.performed -= HandleMoveInput;
            playerInputMaster.Move.canceled -= HandleMoveInput;

            playerInputMaster.Jump.started -= HandlePlayerPressJump;
            playerInputMaster.Jump.performed -= HandlePlayerPressJump;
            playerInputMaster.Jump.canceled -= HandlePlayerPressJump;

            playerInputMaster.AbilityPrimary.started -= HandlePlayerPressAbilityPrimary;
            playerInputMaster.AbilityPrimary.performed -= HandlePlayerPressAbilityPrimary;
            playerInputMaster.AbilityPrimary.canceled -= HandlePlayerPressAbilityPrimary;

            playerInputMaster.AbilitySecondary.started -= HandlePlayerPressAbilitySecondary;
            playerInputMaster.AbilitySecondary.performed -= HandlePlayerPressAbilitySecondary;
            playerInputMaster.AbilitySecondary.canceled -= HandlePlayerPressAbilitySecondary;
        }

        private bool HasNoDirectionalInput() => ExtensionFunctions.IsNearlyEqual(_coreMovementInput.x, 0) && ExtensionFunctions.IsNearlyEqual(_coreMovementInput.y, 0);

        private void HandlePlayerPressAbilitySecondary(InputAction.CallbackContext context) => _abilitySecondaryKey.UpdateInputData(context);

        private void HandlePlayerPressAbilityPrimary(InputAction.CallbackContext context) => _abilityPrimaryKey.UpdateInputData(context);

        private void HandlePlayerPressJump(InputAction.CallbackContext context) => _jumpKey.UpdateInputData(context);

        private void HandleMoveInput(InputAction.CallbackContext context)
        {
            var path = context.action.activeControl.path;
            var deviceName = context.action.activeControl.displayName;
            CustomInputManager.Instance.UpdateLastUsedDeviceInput(deviceName, path);
        }

        private void UpdateMovementInput()
        {
            _coreMovementInput = CustomInputManager.Instance.PlayerInput.Move.ReadValue<Vector2>();

            if (!HasNoDirectionalInput())
            {
                _lastNonZeroCoreMovementInput = _coreMovementInput;
            }
            else if (HasNoDirectionalInput() && ExtensionFunctions.IsNearlyEqual(_currentStateVelocity, 0))
            {
                _lastNonZeroCoreMovementInput = _coreMovementInput;
            }
        }

        private void ResetFrameInputs()
        {
            _jumpKey.ResetPerFrameInput();
            _abilityPrimaryKey.ResetPerFrameInput();
            _abilitySecondaryKey.ResetPerFrameInput();
        }

        #endregion

        #region Player State

        private void PushPlayerState(PlayerState playerState)
        {
            var playerStateStackCount = _playerStateStack.Count;
            _playerStateStack.Add(playerState);
            OnPlayerStatePushed?.Invoke(playerState);

            if (playerStateStackCount > 0)
            {
                var previousState = _playerStateStack[^2];
                OnPlayerStateChanged?.Invoke(previousState, playerState);
            }
        }

        private void PopPlayerState()
        {
            var topState = _playerStateStack[^1];
            _playerStateStack.RemoveAt(_playerStateStack.Count - 1);
            OnPlayerStatePopped?.Invoke(topState);
            OnPlayerStateChanged?.Invoke(topState, _playerStateStack[^1]);
        }

        #endregion
    }
}