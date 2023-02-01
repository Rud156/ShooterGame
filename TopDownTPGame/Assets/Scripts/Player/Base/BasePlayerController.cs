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
        [SerializeField] private List<PlayerInputRestrictingData> _playerInputRestrictingEffectsData;
        [SerializeField] private List<PlayerEffectsAndInputData> _playerEffectsAndInputData;

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
        private List<PlayerEffectsAndInputModifiers> _playerEffectsInputsModifiers;

        // Custom Ability
        private Ability _currentAbility;
        private List<PlayerInputRestrictingStoreData> _playerInputRestrictingEffects;

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
            _playerInputRestrictingEffects = new List<PlayerInputRestrictingStoreData>();
            _playerEffectsInputsModifiers = new List<PlayerEffectsAndInputModifiers>();

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
            // Handle Fixed General Ability Update
            DelegateFixedUpdateAbilities();

            // These inputs are global and should always be processed...
            UpdateCustomAbilityEffects();
            UpdatePlayerEffectsAndInputModifiers();

            // Since this is where input handling happens. Input to be delayed can be done here...
            var isInputRestricted = (int)_playerStateStack[^1] >= (int)PlayerState.CustomInputRestrictingStates;
            if (!isInputRestricted)
            {
                UpdateGroundedState();
                ProcessConstantSpeedFall();
                ProcessJumpInput();
                CheckAndActivateCustomMovementAbility();
                UpdatePlayerMovement();

                if (_playerStateStack[^1] != PlayerState.CustomMovement)
                {
                    ProcessGlobalGravity();
                }

                ApplyFinalMovement();
                CheckAndActivateOtherAbilities();
                UpdateCustomAbilities();
            }

            MarkFrameInputsAsRead();
        }

        #endregion Unity Functions

        #region Player Input Restricting States

        private void UpdateCustomAbilityEffects()
        {
            if (_playerStateStack[^1] != PlayerState.CustomInputRestrictingStates)
            {
                return;
            }

            for (var i = _playerInputRestrictingEffects.Count - 1; i >= 0; i--)
            {
                var customAbility = _playerInputRestrictingEffects[i];
                customAbility.TickDuration();
                _playerInputRestrictingEffects[i] = customAbility;

                switch (_playerInputRestrictingEffects[i].TargetState)
                {
                    case PlayerInputRestrictingState.Frozen:
                    case PlayerInputRestrictingState.Stun:
                        break;

                    case PlayerInputRestrictingState.Knockback:
                    {
                        _characterVelocity = _playerInputRestrictingEffects[i].CustomEffectVectorData1;
                        _characterController.Move(_characterVelocity * Time.fixedDeltaTime);
                    }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (_playerInputRestrictingEffects[i].CustomEffectDuration <= 0)
                {
                    switch (_playerInputRestrictingEffects[i].TargetState)
                    {
                        case PlayerInputRestrictingState.Frozen:
                        case PlayerInputRestrictingState.Knockback:
                        case PlayerInputRestrictingState.Stun:
                        {
                            DestroyPlayerInputCustomEffect(_playerInputRestrictingEffects[i]);
                            _playerInputRestrictingEffects.RemoveAt(i);
                        }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            if (_playerInputRestrictingEffects.Count <= 0)
            {
                PopPlayerState();
            }
        }

        private PlayerInputRestrictingData GetCustomEffectForInputRestrictingState(PlayerInputRestrictingState playerState)
        {
            foreach (var effect in _playerInputRestrictingEffectsData)
            {
                if (effect.targetState == playerState)
                {
                    return effect;
                }
            }

            throw new Exception("Invalid State Requested");
        }

        private void SetupPlayerInputRestrictingState()
        {
            var topState = _playerStateStack[^1];
            if (topState != PlayerState.CustomInputRestrictingStates)
            {
                PushPlayerState(PlayerState.CustomInputRestrictingStates);
            }
        }

        private void DestroyPlayerInputCustomEffect(PlayerInputRestrictingStoreData playerCustomEffectOutput)
        {
            if (playerCustomEffectOutput.Effect != null)
            {
                if (playerCustomEffectOutput.Effect.TryGetComponent(out DestroyParticleEffectSlowlyEmission emission))
                {
                    emission.DestroyEffect();
                }
                else
                {
                    Destroy(playerCustomEffectOutput.Effect);
                }
            }
        }

        private GameObject SpawnGenericInputRestrictingPrefab(PlayerInputRestrictingState stateType)
        {
            var customEffect = GetCustomEffectForInputRestrictingState(stateType);
            var characterTransform = transform;
            var effect = Instantiate(customEffect.effectPrefab, characterTransform.position, Quaternion.identity, characterTransform);
            effect.transform.localPosition += customEffect.effectSpawnOffset;
            return effect;
        }

        public void FreezeCharacter(float abilityDuration)
        {
            SetupPlayerInputRestrictingState();

            var effect = SpawnGenericInputRestrictingPrefab(PlayerInputRestrictingState.Frozen);
            _playerInputRestrictingEffects.Add(
                new PlayerInputRestrictingStoreData()
                {
                    Effect = effect,
                    TargetState = PlayerInputRestrictingState.Frozen,
                    CustomEffectDuration = abilityDuration
                }
            );
        }

        public void KnockbackCharacter(float knockbackDuration, Vector3 knockbackForce)
        {
            SetupPlayerInputRestrictingState();

            var effect = SpawnGenericInputRestrictingPrefab(PlayerInputRestrictingState.Knockback);
            _playerInputRestrictingEffects.Add(
                new PlayerInputRestrictingStoreData()
                {
                    Effect = effect,
                    TargetState = PlayerInputRestrictingState.Knockback,
                    CustomEffectDuration = knockbackDuration,
                    CustomEffectVectorData1 = knockbackForce,
                }
            );
        }

        public void StunCharacter(float duration)
        {
            SetupPlayerInputRestrictingState();

            var effect = SpawnGenericInputRestrictingPrefab(PlayerInputRestrictingState.Stun);
            _playerInputRestrictingEffects.Add(
                new PlayerInputRestrictingStoreData()
                {
                    Effect = effect,
                    TargetState = PlayerInputRestrictingState.Stun,
                    CustomEffectDuration = duration,
                }
            );
        }

        #endregion Player Input Restricting States

        #region Player Effects and Input Modifiers

        private void UpdatePlayerEffectsAndInputModifiers()
        {
            for (var i = _playerEffectsInputsModifiers.Count - 1; i >= 0; i--)
            {
                var movementModifier = _playerEffectsInputsModifiers[i];
                if (movementModifier.IsTimed)
                {
                    movementModifier.CurrentDuration -= Time.fixedDeltaTime;
                    if (movementModifier.CurrentDuration <= 0)
                    {
                        RemoveEffectsAndInputModifierEffects(_playerEffectsInputsModifiers[i]);
                        _playerEffectsInputsModifiers.RemoveAt(i);
                    }
                    else
                    {
                        _playerEffectsInputsModifiers[i] = movementModifier;
                    }
                }
                else if (_playerEffectsInputsModifiers[i].ModifierType == PlayerEffectsAndInputModifierType.ConstantSpeedFall && _isGrounded)
                {
                    RemoveEffectsAndInputModifierEffects(_playerEffectsInputsModifiers[i]);
                    _playerEffectsInputsModifiers.RemoveAt(i);
                }
            }
        }

        private void CheckAndRemovePlayerEffectsAndInputsModifier(string identifier)
        {
            for (var i = 0; i < _playerEffectsInputsModifiers.Count; i++)
            {
                if (string.Equals(_playerEffectsInputsModifiers[i].ModifierIdentifier, identifier))
                {
                    RemoveEffectsAndInputModifierEffects(_playerEffectsInputsModifiers[i]);
                    _playerEffectsInputsModifiers.RemoveAt(i);
                }
            }
        }

        private void RemoveEffectsAndInputModifierEffects(PlayerEffectsAndInputModifiers effectsAndInputModifiers)
        {
            if (effectsAndInputModifiers.Effect != null)
            {
                if (effectsAndInputModifiers.Effect.TryGetComponent(out DestroyParticleEffectSlowlyEmission emission))
                {
                    emission.DestroyEffect();
                }
                else
                {
                    Destroy(effectsAndInputModifiers.Effect);
                }
            }
        }

        public void PlayerConstantSpeedFallTimed(float duration, float multiplier)
        {
            if (_isGrounded)
            {
                return;
            }

            _playerEffectsInputsModifiers.Add(
                new PlayerEffectsAndInputModifiers()
                {
                    CurrentDuration = duration,
                    IsTimed = true,
                    FloatModifierAmount = multiplier,
                    ModifierType = PlayerEffectsAndInputModifierType.ConstantSpeedFall,
                    ModifierIdentifier = string.Empty,
                }
            );
        }

        public void PlayerEnabledParanoia(float duration)
        {
            var effect = SpawnGenericEffectsAndInputPrefab(PlayerEffectsAndInputModifierType.Paranoia);
            _playerEffectsInputsModifiers.Add(
                new PlayerEffectsAndInputModifiers()
                {
                    Effect = effect,
                    CurrentDuration = duration,
                    IsTimed = true,
                    ModifierType = PlayerEffectsAndInputModifierType.Paranoia,
                    ModifierIdentifier = string.Empty,
                }
            );
        }

        public void CharacterEnableEngineerShield(float duration)
        {
            var effect = SpawnGenericEffectsAndInputPrefab(PlayerEffectsAndInputModifierType.EngineerShield);
            _playerEffectsInputsModifiers.Add(
                new PlayerEffectsAndInputModifiers()
                {
                    Effect = effect,
                    CurrentDuration = duration,
                    IsTimed = true,
                    ModifierType = PlayerEffectsAndInputModifierType.EngineerShield,
                    ModifierIdentifier = string.Empty,
                }
            );
        }

        private GameObject SpawnGenericEffectsAndInputPrefab(PlayerEffectsAndInputModifierType effectsAndInputModifierType)
        {
            var customEffect = GetCustomEffectForEffectsAndInputModifierState(effectsAndInputModifierType);
            var parent = customEffect.effectParent;
            if (parent == null)
            {
                parent = transform;
            }

            var effect = Instantiate(customEffect.effectPrefab, parent.position, Quaternion.identity, parent);
            effect.transform.localPosition += customEffect.effectSpawnOffset;
            return effect;
        }

        private PlayerEffectsAndInputData GetCustomEffectForEffectsAndInputModifierState(PlayerEffectsAndInputModifierType effectsAndInputModifierType)
        {
            foreach (var effect in _playerEffectsAndInputData)
            {
                if (effect.modifierType == effectsAndInputModifierType)
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
                _playerEffectsInputsModifiers.Add(
                    new PlayerEffectsAndInputModifiers()
                    {
                        IsTimed = false,
                        ModifierType = PlayerEffectsAndInputModifierType.ConstantSpeedFall,
                        ModifierIdentifier = MovementHoldIdentifier,
                        FloatModifierAmount = _constantSpeedFallMultiplier,
                    }
                );
            }
            else if (_constantSpeedFallKey.KeyReleasedThisFrame || !_constantSpeedFallKey.KeyPressed)
            {
                CheckAndRemovePlayerEffectsAndInputsModifier(MovementHoldIdentifier);
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

                case PlayerState.CustomInputRestrictingStates:
                    throw new Exception("Invalid State Inside Movement");

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
            if (_currentAbility == null)
            {
                PopPlayerState();
                return;
            }

            _currentAbility.AbilityUpdate(this);
            _characterVelocity = _currentAbility.GetMovementData();
            if (_currentAbility.AbilityNeedsToEnd(this))
            {
                _currentAbility.EndAbility(this);
                OnPlayerAbilityEnded?.Invoke(_currentAbility);
                _currentAbility = null;
                PopPlayerState();
            }
        }

        #endregion Movement

        #region Core Movement

        private void CheckAndActivateCustomMovementAbility()
        {
            if (_currentAbility != null)
            {
                return;
            }

            foreach (var ability in _playerAbilities)
            {
                var key = GetKeyForAbilityTrigger(ability.GetAbilityTrigger());

                if (ability.GetAbilityType() == AbilityType.Movement &&
                    ability.AbilityCanStart(this) &&
                    key.KeyPressedThisFrame &&
                    _currentAbility == null)
                {
                    _currentAbility = ability;

                    _currentAbility.StartAbility(this);
                    OnPlayerAbilityStarted?.Invoke(_currentAbility);
                    PushPlayerState(PlayerState.CustomMovement);
                    break;
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
                airMovement = _airControlMultiplier * _currentStateVelocity * airMovement.normalized;

                airMovement.x += _characterVelocity.x;
                airMovement.z += _characterVelocity.z;
                airMovement = airMovement.normalized * _currentStateVelocity;

                _characterVelocity.x = airMovement.x;
                _characterVelocity.z = airMovement.z;
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
            if (isGrounded && !_isGrounded)
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
                    float constantSpeedFall = 0;
                    var hasValue = false;
                    for (var i = 0; i < _playerEffectsInputsModifiers.Count; i++)
                    {
                        if (_playerEffectsInputsModifiers[i].ModifierType == PlayerEffectsAndInputModifierType.ConstantSpeedFall)
                        {
                            constantSpeedFall += _playerEffectsInputsModifiers[i].FloatModifierAmount;
                            hasValue = true;
                        }
                    }

                    if (hasValue)
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
        public float GetCurrentStateVelocity() => _currentStateVelocity;

        #endregion Core Movement

        #region Non Movement Abilities

        private void CheckAndActivateOtherAbilities()
        {
            if (_currentAbility != null)
            {
                return;
            }

            foreach (var ability in _playerAbilities)
            {
                var abilityTrigger = ability.GetAbilityTrigger();
                var key = GetKeyForAbilityTrigger(abilityTrigger);

                if (ability.GetAbilityType() != AbilityType.Movement &&
                    ability.AbilityCanStart(this) &&
                    _currentAbility == null &&
                    key.KeyPressedThisFrame)
                {
                    _currentAbility = ability;

                    _currentAbility.StartAbility(this);
                    OnPlayerAbilityStarted?.Invoke(_currentAbility);
                    break;
                }
            }
        }

        private void UpdateCustomAbilities()
        {
            // This means there is an ability not active OR the ability active is only for Movement
            if (_currentAbility == null || _playerStateStack[^1] == PlayerState.CustomMovement)
            {
                return;
            }

            _currentAbility.AbilityUpdate(this);
            if (_currentAbility.AbilityNeedsToEnd(this))
            {
                _currentAbility.EndAbility(this);
                OnPlayerAbilityEnded?.Invoke(_currentAbility);
                _currentAbility = null;
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

        #endregion Non Movement Abilities

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
                _ => throw new Exception("Invalid Trigger Type"),
            };
        }

        public Vector2 GetCoreMoveInput() => _coreMoveInput;

        #endregion New Input System

        #region Structs

        [Serializable]
        private struct PlayerInputRestrictingData
        {
            public GameObject effectPrefab;
            public Vector3 effectSpawnOffset;
            public PlayerInputRestrictingState targetState;
        }

        private struct PlayerInputRestrictingStoreData
        {
            public GameObject Effect;
            public PlayerInputRestrictingState TargetState;
            public float CustomEffectDuration;
            public float CustomEffectFloatData1;
            public Vector3 CustomEffectVectorData1;
            public void TickDuration() => CustomEffectDuration -= Time.fixedDeltaTime;
        }

        private struct PlayerEffectsAndInputModifiers
        {
            public GameObject Effect;
            public string ModifierIdentifier;
            public PlayerEffectsAndInputModifierType ModifierType;
            public bool IsTimed;
            public float CurrentDuration;
            public float FloatModifierAmount;
        }

        [Serializable]
        private struct PlayerEffectsAndInputData
        {
            public GameObject effectPrefab;
            public Vector3 effectSpawnOffset;
            public Transform effectParent;
            public PlayerEffectsAndInputModifierType modifierType;
        }

        #endregion Structs

        #region Enums

        private enum PlayerInputRestrictingState
        {
            Frozen,
            Knockback,
            Stun,
        }

        private enum PlayerEffectsAndInputModifierType
        {
            ConstantSpeedFall,
            Paranoia,
            EngineerShield,
        }

        #endregion Enums
    }
}