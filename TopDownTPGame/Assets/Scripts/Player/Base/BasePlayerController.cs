#region

using System;
using System.Collections.Generic;
using Effects;
using Player.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils.Input;
using Utils.Misc;
using World;

#endregion

namespace Player.Base
{
    [RequireComponent(typeof(CharacterController))]
    public class BasePlayerController : MonoBehaviour
    {
        private const string ConstantSpeedFallHoldIdentifier = "ConstantSpeedFallHold";

        [Header("Basic Move")]
        [SerializeField] private float _runSpeed;
        [SerializeField] private float _runAccelerationSpeed;
        [SerializeField] private float _walkSpeed;
        [SerializeField] private float _walkAccelerationSpeed;
        [SerializeField] private float _decelerationSpeed;
        [SerializeField] private float _airControlMultiplier;
        [SerializeField] private float _gravityMultiplier;

        [Header("Character Position Points")]
        [SerializeField] private Transform _headTransform;
        [SerializeField] private Transform _bodyTransform;
        [SerializeField] private Transform _legsTransform;

        [Header("Ground Check")]
        [SerializeField] private Transform _groundedCheckPoint;
        [SerializeField] private float _groundedCheckRadius;
        [SerializeField] private LayerMask _groundedCheckMask;
        [SerializeField] private float _groundedTriggerVelocityThreshold;

        [Header("Jump")]
        [SerializeField] private float _jumpVelocity;

        [Header("Abilities")]
        [SerializeField] private List<Ability> _playerAbilities;

        [Header("Custom Effects")]
        [SerializeField] private List<PlayerInputModifierData> _playerInputModifierData;

        // Input
        private Vector2 _coreMoveInput;
        private Vector2 _lastNonZeroCoreInput;
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

        // This is the Acceleration/Deceleration Target
        // Current Start Velocity will not be directly set but instead slowly increased or decreased
        // Based on Target State Velocity
        private float _startStateVelocity;
        private float _targetStateVelocity;
        private float _currentAccelerationSpeed;
        private float _currentStateVelocity;
        private Vector3 _characterVelocity;

        // Jump Data
        private bool _jumpReset;

        // Grounded Data
        public float GravityMultiplier => _gravityMultiplier;
        public bool IsGrounded { get; private set; }
        public float GroundedVelocityThreshold => _groundedTriggerVelocityThreshold;
        public Vector3 CharacterVelocity => _characterVelocity;

        // Movement Modifiers
        private List<PlayerInputModifiers> _playerActiveInputsModifiers;

        // Custom Ability
        private List<Ability> _currentActiveAbilities;
        private List<Ability> _abilitiesToAddNextFrame;

        // Camera
        private Transform _cinemachineControllerTransform;

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
            WorldTimeManager.Instance.OnWorldCustomFixedUpdate += PlayerFixedUpdateFunctions;

            _characterController = GetComponent<CharacterController>();
            _playerStateStack = new List<PlayerState>();
            _playerActiveInputsModifiers = new List<PlayerInputModifiers>();
            _currentActiveAbilities = new List<Ability>();
            _abilitiesToAddNextFrame = new List<Ability>();

            _coreMoveInput = Vector2.zero;
            _lastNonZeroCoreInput = Vector2.zero;
            _runKey = new PlayerInputKey { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };
            _jumpKey = new PlayerInputKey { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };
            _abilityPrimaryKey = new PlayerInputKey { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };
            _abilitySecondaryKey = new PlayerInputKey { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };
            _abilityTertiaryKey = new PlayerInputKey { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };
            _abilityUltimateKey = new PlayerInputKey { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };
            _constantSpeedFallKey = new PlayerInputKey { KeyPressed = false, KeyReleasedThisFrame = false, KeyPressedThisFrame = false };

            _currentStateVelocity = 0;
            _targetStateVelocity = 0;
            _startStateVelocity = 0;
            _cinemachineControllerTransform = GameObject.FindGameObjectWithTag(TagManager.PlayerCinemachineController).transform;

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
            WorldTimeManager.Instance.OnWorldCustomFixedUpdate -= PlayerFixedUpdateFunctions;
        }

        private void Update()
        {
            UpdateKeyboardInput();
            DelegateUpdateAbilities();
        }

        private void PlayerFixedUpdateFunctions(float fixedUpdateTime)
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
            for (var i = _playerActiveInputsModifiers.Count - 1; i >= 0; i--)
            {
                var movementModifier = _playerActiveInputsModifiers[i];
                if (movementModifier.IsTimed)
                {
                    movementModifier.CurrentDuration -= WorldTimeManager.Instance.FixedUpdateTime;
                    if (movementModifier.CurrentDuration <= 0)
                    {
                        RemoveInputModifierEffects(_playerActiveInputsModifiers[i]);
                        _playerActiveInputsModifiers.RemoveAt(i);
                    }
                    else
                    {
                        _playerActiveInputsModifiers[i] = movementModifier;
                    }
                }
            }
        }

        private void CheckAndRemovePlayerAndInputsModifier(string identifier)
        {
            for (var i = 0; i < _playerActiveInputsModifiers.Count; i++)
            {
                if (string.Equals(_playerActiveInputsModifiers[i].ModifierIdentifier, identifier))
                {
                    RemoveInputModifierEffects(_playerActiveInputsModifiers[i]);
                    _playerActiveInputsModifiers.RemoveAt(i);
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

        public int HasActiveInputModifierType(PlayerInputModifierType modifierType)
        {
            for (var i = 0; i < _playerActiveInputsModifiers.Count; i++)
            {
                if (_playerActiveInputsModifiers[i].ModifierType == modifierType)
                {
                    return i;
                }
            }

            return -1;
        }

        private int HasInputModifierType(PlayerInputModifierType modifierType)
        {
            for (var i = 0; i < _playerInputModifierData.Count; i++)
            {
                if (_playerInputModifierData[i].modifierType == modifierType)
                {
                    return i;
                }
            }

            return -1;
        }

        public void PlayerConstantSpeedFallTimed(float duration)
        {
            var constantSpeedFallIndex = HasInputModifierType(PlayerInputModifierType.ConstantSpeedFall);
            if (constantSpeedFallIndex == -1 || IsGrounded)
            {
                return;
            }

            var constantSpeedFallData = _playerInputModifierData[constantSpeedFallIndex];
            _playerActiveInputsModifiers.Add(
                new PlayerInputModifiers()
                {
                    CurrentDuration = duration,
                    IsTimed = true,
                    FloatModifierAmount = constantSpeedFallData.inputModifierAmount,
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
            var constantSpeedFallIndex = HasInputModifierType(PlayerInputModifierType.ConstantSpeedFall);
            if (constantSpeedFallIndex == -1 || IsGrounded)
            {
                return;
            }

            if (_constantSpeedFallKey.KeyPressedThisFrame)
            {
                var constantSpeedFallData = _playerInputModifierData[constantSpeedFallIndex];
                var effect = SpawnGenericInputEffectPrefab(PlayerInputModifierType.ConstantSpeedFall);

                _playerActiveInputsModifiers.Add(
                    new PlayerInputModifiers()
                    {
                        Effect = effect,
                        IsTimed = false,
                        ModifierType = PlayerInputModifierType.ConstantSpeedFall,
                        ModifierIdentifier = ConstantSpeedFallHoldIdentifier,
                        FloatModifierAmount = constantSpeedFallData.inputModifierAmount,
                    }
                );
            }
            else if (_constantSpeedFallKey.KeyReleasedThisFrame || !_constantSpeedFallKey.KeyPressed)
            {
                CheckAndRemovePlayerAndInputsModifier(ConstantSpeedFallHoldIdentifier);
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

            UpdateCurrentToTargetVelocity();
            UpdateCoreMovement();
        }

        private void UpdateIdleState()
        {
            _targetStateVelocity = 0;
            if (!HasNoDirectionalInput())
            {
                _startStateVelocity = _currentStateVelocity;
                PushPlayerState(PlayerState.Walking);
            }
        }

        private void UpdateWalkingState()
        {
            _targetStateVelocity = _walkSpeed;
            _currentAccelerationSpeed = _walkAccelerationSpeed;
            if (_runKey.KeyPressedThisFrame && IsGrounded && _coreMoveInput.y > 0)
            {
                _startStateVelocity = _currentStateVelocity;
                PushPlayerState(PlayerState.Running);
            }
            else if (HasNoDirectionalInput())
            {
                _startStateVelocity = _currentStateVelocity;
                PopPlayerState();
            }
        }

        private void UpdateRunningState()
        {
            _targetStateVelocity = _runSpeed;
            _currentAccelerationSpeed = _runAccelerationSpeed;
            if (HasNoDirectionalInput() || _coreMoveInput.y <= 0 || _runKey.KeyReleasedThisFrame || !_runKey.KeyPressed)
            {
                _startStateVelocity = _currentStateVelocity;
                PopPlayerState();
            }
        }

        private void UpdateFallingState()
        {
            // This is only possible if the Second State is Falling. Since the Idle state is never removed
            // Which means the player jumped from a standstill
            if (_playerStateStack.Count <= 2)
            {
                _targetStateVelocity = _walkSpeed;
                _currentAccelerationSpeed = _walkAccelerationSpeed;
            }

            if (IsGrounded)
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

        private void UpdateCurrentToTargetVelocity()
        {
            // This means we need to Decelerate
            if (_currentStateVelocity > _targetStateVelocity)
            {
                _currentStateVelocity -= _decelerationSpeed * WorldTimeManager.Instance.FixedUpdateTime;
                if (_currentStateVelocity < _targetStateVelocity)
                {
                    _currentStateVelocity = _targetStateVelocity;
                }
            }
            // This means we need to Accelerate
            else if (_currentStateVelocity < _targetStateVelocity)
            {
                _currentStateVelocity += _currentAccelerationSpeed * WorldTimeManager.Instance.FixedUpdateTime;
                if (_currentStateVelocity > _targetStateVelocity)
                {
                    _currentStateVelocity = _targetStateVelocity;
                }
            }
        }

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
                var groundedMovement = forward * _lastNonZeroCoreInput.y + right * _lastNonZeroCoreInput.x;
                groundedMovement.y = 0;
                groundedMovement = _currentStateVelocity * groundedMovement.normalized;

                _characterVelocity.x = groundedMovement.x;
                _characterVelocity.z = groundedMovement.z;
            }
            else
            {
                var airMovement = forward * _lastNonZeroCoreInput.y + right * _lastNonZeroCoreInput.x;
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
            if (!isValidJumpPressed || !_jumpReset)
            {
                return;
            }

            _characterVelocity.y = _jumpVelocity;
            _jumpReset = false;
            OnPlayerJumped?.Invoke();
        }

        private void UpdateGroundedState()
        {
            var isGrounded = Physics.CheckSphere(_groundedCheckPoint.position, _groundedCheckRadius, _groundedCheckMask);
            if (isGrounded)
            {
                _jumpReset = true;
            }

            // Only call the event when the status changes and velocity is large enough
            var yVelocity = Mathf.Abs(_characterVelocity.y);
            if (isGrounded != IsGrounded && yVelocity >= _groundedTriggerVelocityThreshold)
            {
                OnPlayerGroundedChanged?.Invoke(IsGrounded, isGrounded);
            }

            // Set a default gravity when the player is on the ground...
            if (!IsGrounded && isGrounded)
            {
                _characterVelocity.y = Physics.gravity.y * _gravityMultiplier;
            }

            if (!isGrounded && _playerStateStack[^1] != PlayerState.Falling && _playerStateStack[^1] != PlayerState.CustomMovement)
            {
                PushPlayerState(PlayerState.Falling);
            }

            if (isGrounded)
            {
                CheckAndRemovePlayerAndInputsModifier(ConstantSpeedFallHoldIdentifier);
            }

            IsGrounded = isGrounded;
        }

        private void ProcessGlobalGravity()
        {
            if (!IsGrounded)
            {
                if (_characterVelocity.y < 0)
                {
                    var constantSpeedFallIndex = HasActiveInputModifierType(PlayerInputModifierType.ConstantSpeedFall);
                    var constantSpeedFall = constantSpeedFallIndex != -1 ? _playerActiveInputsModifiers[constantSpeedFallIndex].FloatModifierAmount : 0;

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

        private void ApplyFinalMovement() => _characterController.Move(_characterVelocity * WorldTimeManager.Instance.FixedUpdateTime);

        public float GetWalkSpeed() => _walkSpeed;

        public float GetRunSpeed() => _runSpeed;

        public float GetStartStateVelocity() => _startStateVelocity;

        public float GetTargetStateVelocity() => _targetStateVelocity;

        public float GetCurrentStateVelocity() => _currentStateVelocity;

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
                    RepositionAbilities(ability.transform, ability.GetAbilityPositioning());
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
                    RepositionAbilities(ability.transform, ability.GetAbilityPositioning());
                    OnPlayerAbilityStarted?.Invoke(ability);
                    _currentActiveAbilities.Add(ability);
                }
            }

            _abilitiesToAddNextFrame.Clear();
        }

        private void RepositionAbilities(Transform abilityGameObject, AbilityPositioning abilityPositioning)
        {
            switch (abilityPositioning)
            {
                case AbilityPositioning.None:
                    break;

                case AbilityPositioning.Head:
                {
                    abilityGameObject.SetParent(_headTransform);
                    abilityGameObject.localPosition = Vector3.zero;
                }
                    break;

                case AbilityPositioning.Camera:
                {
                    abilityGameObject.SetParent(_cinemachineControllerTransform);
                    abilityGameObject.localPosition = Vector3.zero;
                }
                    break;

                case AbilityPositioning.HeadOrCamera:
                {
                    /*
                     * if LocalPlayer then use _cinemachineControllerTransform else use _headTransform
                     */
                    abilityGameObject.SetParent(_cinemachineControllerTransform);
                    abilityGameObject.localPosition = Vector3.zero;
                }
                    break;

                case AbilityPositioning.Body:
                {
                    abilityGameObject.SetParent(_bodyTransform);
                    abilityGameObject.localPosition = Vector3.zero;
                }
                    break;

                case AbilityPositioning.Legs:
                {
                    abilityGameObject.SetParent(_legsTransform);
                    abilityGameObject.localPosition = Vector3.zero;
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityPositioning), abilityPositioning, null);
            }
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

        public PlayerState GetTopPlayerState() => _playerStateStack[^1];

        #endregion Player State

        #region New Input System

        private void InitializeInputEvents()
        {
            CustomInputManager.Instance.EnablePlayerControls();

            var playerInputMaster = CustomInputManager.Instance.PlayerInput;

            playerInputMaster.Move.started += HandleKeyboardInput;
            playerInputMaster.Move.performed += HandleKeyboardInput;
            playerInputMaster.Move.canceled += HandleKeyboardInput;

            playerInputMaster.Jump.started += HandlePlayerPressJump;
            playerInputMaster.Jump.performed += HandlePlayerPressJump;
            playerInputMaster.Jump.canceled += HandlePlayerPressJump;

            playerInputMaster.Run.started += HandlePlayerPressRun;
            playerInputMaster.Run.performed += HandlePlayerPressRun;
            playerInputMaster.Run.canceled += HandlePlayerPressRun;

            playerInputMaster.ConstantSpeedFall.started += HandlePlayerPressConstantSpeedFall;
            playerInputMaster.ConstantSpeedFall.performed += HandlePlayerPressConstantSpeedFall;
            playerInputMaster.ConstantSpeedFall.canceled += HandlePlayerPressConstantSpeedFall;

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
            var playerInputMaster = CustomInputManager.Instance.PlayerInput;

            playerInputMaster.Move.started -= HandleKeyboardInput;
            playerInputMaster.Move.performed -= HandleKeyboardInput;
            playerInputMaster.Move.canceled -= HandleKeyboardInput;

            playerInputMaster.Jump.started -= HandlePlayerPressJump;
            playerInputMaster.Jump.performed -= HandlePlayerPressJump;
            playerInputMaster.Jump.canceled -= HandlePlayerPressJump;

            playerInputMaster.Run.started -= HandlePlayerPressRun;
            playerInputMaster.Run.performed -= HandlePlayerPressRun;
            playerInputMaster.Run.canceled -= HandlePlayerPressRun;

            playerInputMaster.ConstantSpeedFall.started -= HandlePlayerPressConstantSpeedFall;
            playerInputMaster.ConstantSpeedFall.performed -= HandlePlayerPressConstantSpeedFall;
            playerInputMaster.ConstantSpeedFall.canceled -= HandlePlayerPressConstantSpeedFall;

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

            CustomInputManager.Instance.DisablePlayerControls();
        }

        private void HandleKeyboardInput(InputAction.CallbackContext context)
        {
            var path = context.action.activeControl.path;
            var deviceName = context.action.activeControl.displayName;
            CustomInputManager.Instance.UpdateLastUsedDeviceInput(deviceName, path);
        }

        private void UpdateKeyboardInput()
        {
            _coreMoveInput = CustomInputManager.Instance.PlayerInput.Move.ReadValue<Vector2>();
            if (!HasNoDirectionalInput())
            {
                _lastNonZeroCoreInput = _coreMoveInput;
            }
            else if (HasNoDirectionalInput() && ExtensionFunctions.IsNearlyEqual(_currentStateVelocity, 0))
            {
                _lastNonZeroCoreInput = _coreMoveInput;
            }
        }

        private void HandlePlayerPressJump(InputAction.CallbackContext context) => _jumpKey.UpdateInputData(context);

        private void HandlePlayerPressRun(InputAction.CallbackContext context) => _runKey.UpdateInputData(context);

        private void HandlePlayerPressConstantSpeedFall(InputAction.CallbackContext context) => _constantSpeedFallKey.UpdateInputData(context);

        private void HandlePlayerPressAbilityPrimary(InputAction.CallbackContext context) => _abilityPrimaryKey.UpdateInputData(context);

        private void HandlePlayerPressAbilitySecondary(InputAction.CallbackContext context) => _abilitySecondaryKey.UpdateInputData(context);

        private void HandlePlayerPressAbilityTertiary(InputAction.CallbackContext context) => _abilityTertiaryKey.UpdateInputData(context);

        private void HandlePlayerPressAbilityUltimate(InputAction.CallbackContext context) => _abilityUltimateKey.UpdateInputData(context);

        private void MarkFrameInputsAsRead()
        {
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

        public Vector2 GetLastNonZeroCoreInput() => _lastNonZeroCoreInput;

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
            public float inputModifierAmount;
        }

        #endregion Structs
    }
}