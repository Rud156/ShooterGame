#region

using System;
using System.Collections.Generic;
using Effects;
using Player.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils.Input;
using Utils.Misc;

#endregion

namespace Player.Base
{
    [RequireComponent(typeof(CharacterController))]
    public class BasePlayerController : MonoBehaviour
    {
        private const string MovementHoldIdentifier = "MovementHold";

        [Header("Basic Move")]
        [SerializeField] private float _runSpeed;
        [SerializeField] private float _walkSpeed;
        [SerializeField] private float _airControlMultiplier;
        [SerializeField] private float _gravityMultiplier;

        [Header("Ground Check")]
        [SerializeField] private Transform _groundedCheckPoint;
        [SerializeField] private float _groundedCheckDistance;
        [SerializeField] private LayerMask _groundedCheckMask;

        [Header("Jump")]
        [SerializeField] private float _jumpVelocity;

        [Header("Movement Modifiers")]
        [SerializeField] private bool _constantSpeedFallEnabled;
        [SerializeField] private float _constantSpeedFallMultiplier;

        [Header("Abilities")]
        [SerializeField] private List<Ability> _playerAbilities;

        [Header("Custom Effects")]
        [SerializeField] private List<PlayerInputModifierData> _playerInputModifierData;

        // Input
        private Vector2 _coreMoveInput;
        private PlayerInputKey _runKey;
        private PlayerInputKey _jumpKey;
        private PlayerInputKey _abilityPrimaryKey;
        private PlayerInputKey _abilitySecondaryKey;
        private PlayerInputKey _abilityTertiaryKey;
        private PlayerInputKey _abilityUltimateKey;
        private PlayerInputKey _constantSpeedFallKey;

        // Movement/Controller
        private CharacterController _characterController;
        private List<PlayerState> _playerStateStack;
        private float _currentStateVelocity;
        private Vector3 _characterVelocity;

        public float GravityMultiplier => _gravityMultiplier;
        public bool IsGrounded => _isGrounded;
        private bool _isGrounded;

        // Movement Modifiers
        private List<PlayerInputModifiers> _playerInputsModifiers;

        // Custom Ability
        private List<Ability> _currentActiveAbilities;
        private List<Ability> _abilitiesToAddNextFrame;

        public delegate void PlayerStatePushed(PlayerState newState);
        public delegate void PlayerStatePopped(PlayerState poppedState);
        public delegate void PlayerStateChanged(PlayerState currentState);
        public delegate void PlayerGroundedChange(bool previousState, bool newState);
        public delegate void PlayerJumped();
        public delegate void PlayerAbilityStarted(Ability ability);
        public delegate void PlayerAbilityEnded(Ability ability);

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
            InitializeInputEvents();

            _characterController = GetComponent<CharacterController>();
            _playerStateStack = new List<PlayerState>();
            _playerInputsModifiers = new List<PlayerInputModifiers>();
            _currentActiveAbilities = new List<Ability>();
            _abilitiesToAddNextFrame = new List<Ability>();

            _coreMoveInput = new Vector2();
            _runKey = new PlayerInputKey() { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };
            _jumpKey = new PlayerInputKey() { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };
            _abilityPrimaryKey = new PlayerInputKey() { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };
            _abilitySecondaryKey = new PlayerInputKey() { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };
            _abilityTertiaryKey = new PlayerInputKey() { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };
            _abilityUltimateKey = new PlayerInputKey() { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };
            _constantSpeedFallKey = new PlayerInputKey() { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };

            _currentStateVelocity = 0;

            foreach (var ability in _playerAbilities)
            {
                ability.UnityStartDelegate(this);
            }

            PushPlayerState(PlayerState.Idle);
        }

        private void OnDestroy()
        {
            foreach (var ability in _playerAbilities)
            {
                ability.ClearAllAbilityData(this);
            }

            DeInitializeInputEvents();
        }

        private void Update()
        {
            HandleKeyboardInput();
            DelegateUpdateAbilities();
        }

        private void FixedUpdate()
        {
            // Process these at the beginning to not cause a race condition
            ProcessNextFrameAbilities();

            // Handle Fixed General Ability Update
            DelegateFixedUpdateAbilities();

            // These inputs are global and should always be processed...
            UpdatePlayerInputModifiers();

            UpdateGroundedState();
            if (_playerStateStack[^1] != PlayerState.CustomMovement)
            {
                ProcessJumpInput();
                ProcessGlobalGravity();
                ProcessConstantSpeedFall();
            }

            CheckAndActivateAbilities();
            UpdatePlayerMovement();

            ApplyFinalMovement();
            UpdateCustomAbilities();

            MarkFrameInputsAsRead();
        }

        #endregion Unity Functions

        #region Player Effects and Input Modifiers

        private void UpdatePlayerInputModifiers()
        {
            for (var i = _playerInputsModifiers.Count - 1; i >= 0; i--)
            {
                var movementModifier = _playerInputsModifiers[i];
                if (movementModifier.IsTimed)
                {
                    movementModifier.CurrentDuration -= Time.fixedDeltaTime;
                    if (movementModifier.CurrentDuration <= 0)
                    {
                        RemoveInputModifierEffects(_playerInputsModifiers[i]);
                        _playerInputsModifiers.RemoveAt(i);
                    }
                    else
                    {
                        _playerInputsModifiers[i] = movementModifier;
                    }
                }
                else if (_playerInputsModifiers[i].ModifierType == PlayerInputModifierType.ConstantSpeedFall && _isGrounded)
                {
                    RemoveInputModifierEffects(_playerInputsModifiers[i]);
                    _playerInputsModifiers.RemoveAt(i);
                }
            }
        }

        private void CheckAndRemovePlayerAndInputsModifier(string identifier)
        {
            for (var i = 0; i < _playerInputsModifiers.Count; i++)
            {
                if (string.Equals(_playerInputsModifiers[i].ModifierIdentifier, identifier))
                {
                    RemoveInputModifierEffects(_playerInputsModifiers[i]);
                    _playerInputsModifiers.RemoveAt(i);
                }
            }
        }

        private void RemoveInputModifierEffects(PlayerInputModifiers inputModifiers)
        {
            if (inputModifiers.Effect != null)
            {
                if (inputModifiers.Effect.TryGetComponent(out DestroyParticleEffectSlowlyEmission emission))
                {
                    emission.DestroyEffect();
                }
                else
                {
                    Destroy(inputModifiers.Effect);
                }
            }
        }

        private int HasInputModifierType(PlayerInputModifierType modifierType)
        {
            for (var i = 0; i < _playerInputsModifiers.Count; i++)
            {
                if (_playerInputsModifiers[i].ModifierType == modifierType)
                {
                    return i;
                }
            }

            return -1;
        }

        public void PlayerConstantSpeedFallTimed(float duration, float multiplier)
        {
            if (_isGrounded)
            {
                return;
            }

            _playerInputsModifiers.Add(
                new PlayerInputModifiers()
                {
                    CurrentDuration = duration,
                    IsTimed = true,
                    FloatModifierAmount = multiplier,
                    ModifierType = PlayerInputModifierType.ConstantSpeedFall,
                    ModifierIdentifier = string.Empty,
                }
            );
        }

        private GameObject SpawnGenericInputEffectPrefab(PlayerInputModifierType inputModifierType)
        {
            var customEffect = GetCustomEffectForInputModifierState(inputModifierType);
            var parent = customEffect.effectParent;
            if (parent == null)
            {
                parent = transform;
            }

            var effect = Instantiate(customEffect.effectPrefab, parent.position, Quaternion.identity, parent);
            effect.transform.localPosition += customEffect.effectSpawnOffset;
            return effect;
        }

        private PlayerInputModifierData GetCustomEffectForInputModifierState(PlayerInputModifierType inputModifierType)
        {
            foreach (var effect in _playerInputModifierData)
            {
                if (effect.modifierType == inputModifierType)
                {
                    return effect;
                }
            }

            throw new Exception("Invalid State Requested");
        }

        #region Updates

        private void ProcessConstantSpeedFall()
        {
            if (!_constantSpeedFallEnabled || _isGrounded)
            {
                return;
            }

            if (_constantSpeedFallKey.KeyPressedThisFrame)
            {
                _playerInputsModifiers.Add(
                    new PlayerInputModifiers()
                    {
                        IsTimed = false,
                        ModifierType = PlayerInputModifierType.ConstantSpeedFall,
                        ModifierIdentifier = MovementHoldIdentifier,
                        FloatModifierAmount = _constantSpeedFallMultiplier,
                    }
                );
            }
            else if (_constantSpeedFallKey.KeyReleasedThisFrame || !_constantSpeedFallKey.KeyPressed)
            {
                CheckAndRemovePlayerAndInputsModifier(MovementHoldIdentifier);
            }
        }

        #endregion Updates

        #endregion Player Effects and Input Modifiers

        #region Movement

        private void UpdatePlayerMovement()
        {
            switch (_playerStateStack[^1])
            {
                case PlayerState.Idle:
                    UpdateIdleState();
                    break;

                case PlayerState.Walking:
                    UpdateWalkingState();
                    break;

                case PlayerState.Running:
                    UpdateRunningState();
                    break;

                case PlayerState.Falling:
                    UpdateFallingState();
                    break;

                case PlayerState.CustomMovement:
                    UpdateCustomMovementState();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdateCoreMovement();
        }

        private void UpdateIdleState()
        {
            _currentStateVelocity = 0;
            if (!HasNoDirectionalInput())
            {
                PushPlayerState(PlayerState.Walking);
            }
        }

        private void UpdateWalkingState()
        {
            _currentStateVelocity = _walkSpeed;
            if (_runKey.KeyPressedThisFrame && _isGrounded && _coreMoveInput.y > 0)
            {
                PushPlayerState(PlayerState.Running);
            }
            else if (HasNoDirectionalInput())
            {
                PopPlayerState();
            }
        }

        private void UpdateRunningState()
        {
            _currentStateVelocity = _runSpeed;
            if (HasNoDirectionalInput() || _coreMoveInput.y <= 0 || _runKey.KeyReleasedThisFrame || !_runKey.KeyPressed)
            {
                PopPlayerState();
            }
        }

        private void UpdateFallingState()
        {
            // This is only possible if the Second State is Falling. Since the Idle state is never removed
            // Which means the player jumped from a standstill
            if (_playerStateStack.Count <= 2)
            {
                _currentStateVelocity = _walkSpeed;
            }

            if (_isGrounded)
            {
                PopPlayerState();
            }
        }

        private void UpdateCustomMovementState()
        {
            var hasMovementAbility = false;
            for (var i = _currentActiveAbilities.Count - 1; i >= 0; i--)
            {
                // Don't process Non Movement Abilities here...
                var currentAbility = _currentActiveAbilities[i];
                if (currentAbility.GetAbilityType() != AbilityType.Movement)
                {
                    continue;
                }

                currentAbility.AbilityUpdate(this);
                _characterVelocity = currentAbility.GetMovementData();
                if (currentAbility.AbilityNeedsToEnd(this))
                {
                    currentAbility.EndAbility(this);
                    OnPlayerAbilityEnded?.Invoke(currentAbility);
                    _currentActiveAbilities.RemoveAt(i);
                }
                else
                {
                    hasMovementAbility = true;
                }
            }

            if (_currentActiveAbilities.Count <= 0 || !hasMovementAbility)
            {
                PopPlayerState();
            }
        }

        #endregion Movement

        #region Core Movement

        private void UpdateCoreMovement()
        {
            var characterTransform = transform;
            var forward = characterTransform.forward;
            var right = characterTransform.right;

            // When Custom Apply Movement Directly
            if (_playerStateStack[^1] == PlayerState.CustomMovement)
            {
                // Do nothing here since Custom Movement handles it...
            }
            else if (_playerStateStack[^1] != PlayerState.Falling)
            {
                var groundedMovement = forward * _coreMoveInput.y + right * _coreMoveInput.x;
                groundedMovement.y = 0;
                groundedMovement = _currentStateVelocity * groundedMovement.normalized;

                _characterVelocity.x = groundedMovement.x;
                _characterVelocity.z = groundedMovement.z;
            }
            else
            {
                var airMovement = forward * _coreMoveInput.y + right * _coreMoveInput.x;
                airMovement.y = 0;
                airMovement = airMovement.normalized * (_airControlMultiplier * _currentStateVelocity);

                var clampedXVelocity = Mathf.Clamp(_characterVelocity.x + airMovement.x, -_currentStateVelocity, _currentStateVelocity);
                var clampedZVelocity = Mathf.Clamp(_characterVelocity.z + airMovement.z, -_currentStateVelocity, _currentStateVelocity);

                _characterVelocity.x = clampedXVelocity;
                _characterVelocity.z = clampedZVelocity;
            }
        }

        private void ProcessJumpInput()
        {
            var isValidJumpPressed = _jumpKey.KeyPressedThisFrame;
            if (!isValidJumpPressed || !_isGrounded)
            {
                return;
            }

            _characterVelocity.y += _jumpVelocity;
            OnPlayerJumped?.Invoke();
        }

        private void UpdateGroundedState()
        {
            var isGrounded = Physics.Raycast(_groundedCheckPoint.position, Vector3.down, _groundedCheckDistance, _groundedCheckMask);
            if (isGrounded)
            {
                _characterVelocity.y = 0;
            }

            if (!isGrounded && _playerStateStack[^1] != PlayerState.Falling && _playerStateStack[^1] != PlayerState.CustomMovement)
            {
                PushPlayerState(PlayerState.Falling);
            }

            OnPlayerGroundedChanged?.Invoke(_isGrounded, isGrounded);
            _isGrounded = isGrounded;
        }

        private void ProcessGlobalGravity()
        {
            if (!_isGrounded)
            {
                if (_characterVelocity.y < 0)
                {
                    var constantSpeedFallIndex = HasInputModifierType(PlayerInputModifierType.ConstantSpeedFall);
                    var constantSpeedFall = constantSpeedFallIndex != -1 ? _playerInputsModifiers[constantSpeedFallIndex].FloatModifierAmount : 0;

                    if (constantSpeedFallIndex != -1)
                    {
                        var gravityY = Physics.gravity.y * _gravityMultiplier;
                        _characterVelocity.y = gravityY * constantSpeedFall;
                    }
                    else
                    {
                        _characterVelocity.y += Physics.gravity.y * _gravityMultiplier;
                    }
                }
                else
                {
                    _characterVelocity.y += Physics.gravity.y * _gravityMultiplier;
                }
            }
        }

        private void ApplyFinalMovement() => _characterController.Move(_characterVelocity * Time.fixedDeltaTime);

        public Vector3 GetCharacterVelocity() => _characterVelocity;

        #endregion Core Movement

        #region Ability Functions

        private void CheckAndActivateAbilities()
        {
            foreach (var ability in _playerAbilities)
            {
                var abilityTrigger = ability.GetAbilityTrigger();
                var key = GetKeyForAbilityTrigger(abilityTrigger);

                if (ability.AbilityCanStart(this) && key.KeyPressedThisFrame)
                {
                    if (ability.GetAbilityType() == AbilityType.Movement)
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

                    ability.StartAbility(this);
                    OnPlayerAbilityStarted?.Invoke(ability);
                    _currentActiveAbilities.Add(ability);

                    break;
                }
            }
        }

        private void UpdateCustomAbilities()
        {
            for (var i = _currentActiveAbilities.Count - 1; i >= 0; i--)
            {
                // Do not process Movement Abilities here...
                var currentAbility = _currentActiveAbilities[i];
                if (currentAbility.GetAbilityType() != AbilityType.NonMovement)
                {
                    continue;
                }

                currentAbility.AbilityUpdate(this);
                if (currentAbility.AbilityNeedsToEnd(this))
                {
                    currentAbility.EndAbility(this);
                    OnPlayerAbilityEnded?.Invoke(currentAbility);
                    _currentActiveAbilities.RemoveAt(i);
                }
            }
        }

        public List<Ability> GetActiveAbilities() => _currentActiveAbilities;

        public void CheckAndAddExternalAbility(Ability ability) => _abilitiesToAddNextFrame.Add(ability);

        private void ProcessNextFrameAbilities()
        {
            foreach (var ability in _abilitiesToAddNextFrame)
            {
                // This Ability will already have started 
                if (ability.AbilityCanStart(this))
                {
                    if (ability.GetAbilityType() == AbilityType.Movement)
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
                        if (!activeAbility.HasAbilityNameInAllowedList(activeAbility.GetAbilityNameType()))
                        {
                            activeAbility.EndAbility(this);
                            OnPlayerAbilityEnded?.Invoke(activeAbility);
                            _currentActiveAbilities.RemoveAt(i);
                        }
                    }

                    ability.StartAbility(this);
                    OnPlayerAbilityStarted?.Invoke(ability);
                    _currentActiveAbilities.Add(ability);
                }
            }

            _abilitiesToAddNextFrame.Clear();
        }

        private void RemoveExistingMovementAbility()
        {
            for (var i = _currentActiveAbilities.Count - 1; i >= 0; i--)
            {
                var currentAbility = _currentActiveAbilities[i];
                if (currentAbility.GetAbilityType() == AbilityType.Movement)
                {
                    currentAbility.EndAbility(this);
                    OnPlayerAbilityEnded?.Invoke(currentAbility);
                    _currentActiveAbilities.RemoveAt(i);
                }
            }
        }

        private void DelegateUpdateAbilities()
        {
            foreach (var ability in _playerAbilities)
            {
                ability.UnityUpdateDelegate(this);
            }
        }

        private void DelegateFixedUpdateAbilities()
        {
            foreach (var ability in _playerAbilities)
            {
                ability.UnityFixedUpdateDelegate(this);
            }
        }

        #endregion Ability Functions

        #region Player State

        private void PushPlayerState(PlayerState state)
        {
            _playerStateStack.Add(state);
            OnPlayerStatePushed?.Invoke(state);
            OnPlayerStateChanged?.Invoke(state);
        }

        private void PopPlayerState()
        {
            var topState = _playerStateStack[^1];
            _playerStateStack.RemoveAt(_playerStateStack.Count - 1);
            OnPlayerStatePopped?.Invoke(topState);
            OnPlayerStateChanged?.Invoke(_playerStateStack[^1]);
        }

        #endregion Player State

        #region New Input System

        private void InitializeInputEvents()
        {
            InputManager.Instance.EnablePlayerControls();

            var playerInputMaster = InputManager.Instance.PlayerInput;

            playerInputMaster.Jump.started += HandlePlayerPressJump;
            playerInputMaster.Jump.performed += HandlePlayerPressJump;
            playerInputMaster.Jump.canceled += HandlePlayerPressJump;

            playerInputMaster.Run.started += HandlePlayerPressRun;
            playerInputMaster.Run.performed += HandlePlayerPressRun;
            playerInputMaster.Run.canceled += HandlePlayerPressRun;

            playerInputMaster.MovementHold.started += HandlePlayerPressConstantSpeedFall;
            playerInputMaster.MovementHold.performed += HandlePlayerPressConstantSpeedFall;
            playerInputMaster.MovementHold.canceled += HandlePlayerPressConstantSpeedFall;

            playerInputMaster.AbilityPrimary.started += HandlePlayerPressAbilityPrimary;
            playerInputMaster.AbilityPrimary.performed += HandlePlayerPressAbilityPrimary;
            playerInputMaster.AbilityPrimary.canceled += HandlePlayerPressAbilityPrimary;

            playerInputMaster.AbilitySecondary.started += HandlePlayerPressAbilitySecondary;
            playerInputMaster.AbilitySecondary.performed += HandlePlayerPressAbilitySecondary;
            playerInputMaster.AbilitySecondary.canceled += HandlePlayerPressAbilitySecondary;

            playerInputMaster.AbilityTertiary.started += HandlePlayerPressAbilityTertiary;
            playerInputMaster.AbilityTertiary.performed += HandlePlayerPressAbilityTertiary;
            playerInputMaster.AbilityTertiary.canceled += HandlePlayerPressAbilityTertiary;

            playerInputMaster.AbilityUltimate.started += HandlePlayerPressAbilityUltimate;
            playerInputMaster.AbilityUltimate.performed += HandlePlayerPressAbilityUltimate;
            playerInputMaster.AbilityUltimate.canceled += HandlePlayerPressAbilityUltimate;
        }

        private void DeInitializeInputEvents()
        {
            var playerInputMaster = InputManager.Instance.PlayerInput;

            playerInputMaster.Jump.started -= HandlePlayerPressJump;
            playerInputMaster.Jump.performed -= HandlePlayerPressJump;
            playerInputMaster.Jump.canceled -= HandlePlayerPressJump;

            playerInputMaster.Run.started -= HandlePlayerPressRun;
            playerInputMaster.Run.performed -= HandlePlayerPressRun;
            playerInputMaster.Run.canceled -= HandlePlayerPressRun;

            playerInputMaster.MovementHold.started -= HandlePlayerPressConstantSpeedFall;
            playerInputMaster.MovementHold.performed -= HandlePlayerPressConstantSpeedFall;
            playerInputMaster.MovementHold.canceled -= HandlePlayerPressConstantSpeedFall;

            playerInputMaster.AbilityPrimary.started -= HandlePlayerPressAbilityPrimary;
            playerInputMaster.AbilityPrimary.performed -= HandlePlayerPressAbilityPrimary;
            playerInputMaster.AbilityPrimary.canceled -= HandlePlayerPressAbilityPrimary;

            playerInputMaster.AbilitySecondary.started -= HandlePlayerPressAbilitySecondary;
            playerInputMaster.AbilitySecondary.performed -= HandlePlayerPressAbilitySecondary;
            playerInputMaster.AbilitySecondary.canceled -= HandlePlayerPressAbilitySecondary;

            playerInputMaster.AbilityTertiary.started -= HandlePlayerPressAbilityTertiary;
            playerInputMaster.AbilityTertiary.performed -= HandlePlayerPressAbilityTertiary;
            playerInputMaster.AbilityTertiary.canceled -= HandlePlayerPressAbilityTertiary;

            playerInputMaster.AbilityUltimate.started -= HandlePlayerPressAbilityUltimate;
            playerInputMaster.AbilityUltimate.performed -= HandlePlayerPressAbilityUltimate;
            playerInputMaster.AbilityUltimate.canceled -= HandlePlayerPressAbilityUltimate;

            InputManager.Instance.DisablePlayerControls();
        }

        private void HandleKeyboardInput() =>
            _coreMoveInput = InputManager.Instance.PlayerInput.Move.ReadValue<Vector2>();

        private void HandlePlayerPressJump(InputAction.CallbackContext context) => _jumpKey.UpdateInputData(context);

        private void HandlePlayerPressRun(InputAction.CallbackContext context) => _runKey.UpdateInputData(context);

        private void HandlePlayerPressConstantSpeedFall(InputAction.CallbackContext context) => _constantSpeedFallKey.UpdateInputData(context);

        private void HandlePlayerPressAbilityPrimary(InputAction.CallbackContext context) => _abilityPrimaryKey.UpdateInputData(context);

        private void HandlePlayerPressAbilitySecondary(InputAction.CallbackContext context) => _abilitySecondaryKey.UpdateInputData(context);

        private void HandlePlayerPressAbilityTertiary(InputAction.CallbackContext context) => _abilityTertiaryKey.UpdateInputData(context);

        private void HandlePlayerPressAbilityUltimate(InputAction.CallbackContext context) => _abilityUltimateKey.UpdateInputData(context);

        private void MarkFrameInputsAsRead()
        {
            _coreMoveInput.x = 0;
            _coreMoveInput.y = 0;

            _jumpKey.ResetPerFrameInput();
            _runKey.ResetPerFrameInput();
            _abilityPrimaryKey.ResetPerFrameInput();
            _abilitySecondaryKey.ResetPerFrameInput();
            _abilityTertiaryKey.ResetPerFrameInput();
            _abilityUltimateKey.ResetPerFrameInput();
            _constantSpeedFallKey.ResetPerFrameInput();
        }

        private bool HasNoDirectionalInput() => ExtensionFunctions.IsNearlyEqual(_coreMoveInput.x, 0) && ExtensionFunctions.IsNearlyEqual(_coreMoveInput.y, 0);

        public PlayerInputKey GetKeyForAbilityTrigger(AbilityTrigger abilityTrigger)
        {
            return abilityTrigger switch
            {
                AbilityTrigger.Primary => _abilityPrimaryKey,
                AbilityTrigger.Secondary => _abilitySecondaryKey,
                AbilityTrigger.Tertiary => _abilityTertiaryKey,
                AbilityTrigger.Ultimate => _abilityUltimateKey,
                AbilityTrigger.ExternalAddedAbility => throw new Exception("Invalid Trigger Type"),
                _ => throw new Exception("Invalid Trigger Type"),
            };
        }

        public Vector2 GetCoreMoveInput() => _coreMoveInput;

        #endregion New Input System

        #region Structs

        private struct PlayerInputModifiers
        {
            public GameObject Effect;
            public string ModifierIdentifier;
            public PlayerInputModifierType ModifierType;
            public bool IsTimed;
            public float CurrentDuration;
            public float FloatModifierAmount;
        }

        [Serializable]
        private struct PlayerInputModifierData
        {
            public GameObject effectPrefab;
            public Vector3 effectSpawnOffset;
            public Transform effectParent;
            public PlayerInputModifierType modifierType;
        }

        #endregion Structs

        #region Enums

        private enum PlayerInputModifierType
        {
            ConstantSpeedFall,
        }

        #endregion Enums
    }
}