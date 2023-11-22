using System;
using System.Collections.Generic;
using Player.Abilities;
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

        [Header("Components")]
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private PlayerShootController _playerShootController;
        [SerializeField] private List<AbilityBase> _playerAbilities;

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
        public Vector2 CoreMovementInput => _coreMovementInput;

        // Character Controller
        private CharacterController _characterController;

        // Player Rotation
        private Camera _mainCamera;

        // Player Abilities
        private List<AbilityBase> _currentActiveAbilities;
        private List<AbilityBase> _abilitiesToAddNextFrame; // These are abilities that have to be added next frame so will stop and override incompatible abilities
        public List<AbilityBase> ActiveAbilities => _currentActiveAbilities;

        // General Getters
        public Animator PlayerAnimator => _playerAnimator;
        public PlayerShootController PlayerShootController => _playerShootController;

        // Delegates
        public delegate void PlayerStatePushed(PlayerState newState);
        public delegate void PlayerStatePopped(PlayerState poppedState);
        public delegate void PlayerStateChanged(PlayerState previousState, PlayerState newState);
        public delegate void PlayerGroundedChange(bool previousState, bool newState);
        public delegate void PlayerJumped();
        public delegate void PlayerAbilityStarted(AbilityBase ability);
        public delegate void PlayerAbilityEnded(AbilityBase ability);
        public event PlayerStatePushed OnPlayerStatePushed;
        public event PlayerStatePopped OnPlayerStatePopped;
        public event PlayerStateChanged OnPlayerStateChanged;
        public event PlayerGroundedChange OnPlayerGroundedChanged;
        public event PlayerJumped OnPlayerJumped;
        public event PlayerAbilityStarted OnPlayerAbilityStarted;
        public event PlayerAbilityEnded OnPlayerAbilityEnded;

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
            _jumpReset = true;
            _currentStateVelocity = 0;

            _currentActiveAbilities = new List<AbilityBase>();
            _abilitiesToAddNextFrame = new List<AbilityBase>();
            foreach (var ability in _playerAbilities)
            {
                ability.UnityStartDelegate(this);
            }

            PushPlayerState(PlayerState.Idle);
        }

        private void OnDestroy()
        {
            DeInitializeInputEvents();
            WorldTimeManager.Instance.OnWorldCustomFixedUpdate -= PlayerFixedUpdate;
        }

        private void Update()
        {
            UpdateMovementInput();
            DelegateUnityUpdateAbilities();
            UpdateAbilities();
        }

        private void PlayerFixedUpdate(float fixedUpdateTime)
        {
            ProcessNextFrameAbilities();
            DelegateUnityFixedUpdateAbilities(fixedUpdateTime);

            UpdateGroundedState();
            if (_playerStateStack[^1] != PlayerState.CustomMovement)
            {
                ProcessJumpInput();
                ProcessGlobalGravity();
            }

            CheckAndActivateAbilities();
            UpdatePlayerMovement();

            FixedUpdateAbilities(fixedUpdateTime);
            ApplyFinalMovement(fixedUpdateTime);

            ResetFrameInputs();
        }

        #endregion

        #region Player Movement

        private void UpdateGroundedState()
        {
            var isGrounded = _characterController.isGrounded;

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

        private void ProcessGlobalGravity()
        {
            if (!IsGrounded)
            {
                _characterVelocity.y += Physics.gravity.y * _gravityMultiplier;
            }
            else
            {
                _characterVelocity.y = Physics.gravity.y;
                _jumpReset = true;
            }
        }

        private void UpdatePlayerMovement()
        {
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

                case PlayerState.CustomMovement:
                    UpdateCustomMovementState();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_playerStateStack[^1] != PlayerState.CustomMovement)
            {
                UpdatePlayerRotation();
                UpdateCoreMovement();
            }
        }

        #region Core Movement

        private void UpdatePlayerRotation()
        {
            var inputRotation = CustomInputManager.Instance.PlayerInput.Move.ReadValue<Vector2>();
            if (inputRotation == Vector2.zero)
            {
                return;
            }

            var angle = Mathf.Atan2(inputRotation.x, inputRotation.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }

        private void UpdateCoreMovement()
        {
            var characterTransform = transform;
            var forward = characterTransform.forward;

            if (_playerStateStack[^1] != PlayerState.Falling)
            {
                var groundedMovement = forward * _coreMovementInput.y;

                groundedMovement.y = 0;
                groundedMovement = _currentStateVelocity * groundedMovement.normalized;

                _characterVelocity.x = groundedMovement.x;
                _characterVelocity.z = groundedMovement.z;
            }
            else
            {
                var airMovement = forward * _lastNonZeroCoreMovementInput.y;

                airMovement.y = 0;
                airMovement = airMovement.normalized * (_airMultiplier * _currentStateVelocity);

                var clampedXVelocity = Mathf.Clamp(_characterVelocity.x + airMovement.x, -_currentStateVelocity, _currentStateVelocity);
                var clampedZVelocity = Mathf.Clamp(_characterVelocity.z + airMovement.z, -_currentStateVelocity, _currentStateVelocity);

                _characterVelocity.x = clampedXVelocity;
                _characterVelocity.z = clampedZVelocity;
            }
        }

        #endregion

        private void ApplyFinalMovement(float fixedUpdateTime) => _characterController.Move(_characterVelocity * fixedUpdateTime);

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

        private void UpdateCustomMovementState()
        {
            var hasMovementAbility = false;
            foreach (var ability in _currentActiveAbilities)
            {
                if (ability.IsMovementAbility)
                {
                    _characterVelocity = ability.MovementData();
                    hasMovementAbility = true;
                    break;
                }
            }

            if (!hasMovementAbility || _currentActiveAbilities.Count <= 0)
            {
                PopPlayerState();
            }
        }

        #endregion

        #endregion

        #region Ability Controls

        private void CheckAndActivateAbilities()
        {
            foreach (var ability in _playerAbilities)
            {
                if (ability.AbilityCanStart(this))
                {
                    if (ability.IsMovementAbility)
                    {
                        if (_playerStateStack[^1] != PlayerState.CustomMovement)
                        {
                            PushPlayerState(PlayerState.CustomMovement);
                        }
                        else
                        {
                            RemoveExistingMovementAbility();
                        }
                    }

                    ability.AbilityStart(this);
                    OnPlayerAbilityStarted?.Invoke(ability);
                    _currentActiveAbilities.Add(ability);
                }
            }
        }

        private void ProcessNextFrameAbilities()
        {
            foreach (var ability in _abilitiesToAddNextFrame)
            {
                // This Ability will already have started 
                if (ability.AbilityCanStart(this))
                {
                    if (ability.IsMovementAbility)
                    {
                        if (_playerStateStack[^1] != PlayerState.CustomMovement)
                        {
                            PushPlayerState(PlayerState.CustomMovement);
                        }
                        else
                        {
                            RemoveExistingMovementAbility();
                        }
                    }

                    // Also check for incompatible abilities
                    for (var i = _currentActiveAbilities.Count - 1; i >= 0; i--)
                    {
                        var activeAbility = _currentActiveAbilities[i];
                        if (!activeAbility.HasAbilityNameInDisAllowedList(activeAbility.AbilityNameType))
                        {
                            activeAbility.AbilityEnd(this);
                            OnPlayerAbilityEnded?.Invoke(activeAbility);
                            _currentActiveAbilities.RemoveAt(i);
                        }
                    }

                    ability.AbilityStart(this);
                    OnPlayerAbilityStarted?.Invoke(ability);
                    _currentActiveAbilities.Add(ability);
                }
            }

            _abilitiesToAddNextFrame.Clear();
        }

        private void UpdateAbilities()
        {
            var deltaTime = Time.deltaTime;
            for (var i = _currentActiveAbilities.Count - 1; i >= 0; i--)
            {
                var currentAbility = _currentActiveAbilities[i];
                currentAbility.AbilityUpdate(this, deltaTime);
            }
        }

        private void FixedUpdateAbilities(float fixedDeltaTime)
        {
            for (var i = _currentActiveAbilities.Count - 1; i >= 0; i--)
            {
                var currentAbility = _currentActiveAbilities[i];
                currentAbility.AbilityFixedUpdate(this, fixedDeltaTime);

                if (currentAbility.AbilityNeedsToEnd(this))
                {
                    currentAbility.AbilityEnd(this);
                    OnPlayerAbilityEnded?.Invoke(currentAbility);
                    _currentActiveAbilities.RemoveAt(i);
                }
            }
        }

        private void RemoveExistingMovementAbility()
        {
            for (var i = _currentActiveAbilities.Count - 1; i >= 0; i--)
            {
                var currentAbility = _currentActiveAbilities[i];
                if (currentAbility.IsMovementAbility)
                {
                    currentAbility.AbilityEnd(this);
                    OnPlayerAbilityEnded?.Invoke(currentAbility);
                    _currentActiveAbilities.RemoveAt(i);
                }
            }
        }

        #region Unity Ability Function Delegates

        private void DelegateUnityUpdateAbilities()
        {
            foreach (var ability in _playerAbilities)
            {
                ability.UnityUpdateDelegate(this);
            }
        }

        private void DelegateUnityFixedUpdateAbilities(float fixedDeltaTime)
        {
            foreach (var ability in _playerAbilities)
            {
                ability.UnityFixedUpdateDelegate(this, fixedDeltaTime);
            }
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

        private bool HasNoDirectionalInput() => ExtensionFunctions.IsNearlyZero(_coreMovementInput.y);

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

            // Normalize Input to always move forward and instead Rotate the Character
            var xMovement = Mathf.Abs(_coreMovementInput.x);
            var yMovement = Mathf.Abs(_coreMovementInput.y);
            _coreMovementInput.x = 0;
            _coreMovementInput.y = Mathf.Max(xMovement, yMovement);

            if (!HasNoDirectionalInput())
            {
                _lastNonZeroCoreMovementInput = _coreMovementInput;
            }
            else if (HasNoDirectionalInput() && ExtensionFunctions.IsNearlyZero(_currentStateVelocity))
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

        public PlayerInputKey GetKeyForAbilityTrigger(AbilityTrigger abilityTrigger)
        {
            return abilityTrigger switch
            {
                AbilityTrigger.Primary => _abilityPrimaryKey,
                AbilityTrigger.Secondary => _abilitySecondaryKey,
                AbilityTrigger.ExternalAddedAbility => throw new Exception("Invalid Trigger Type"),
                _ => throw new Exception("Invalid Trigger Type"),
            };
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