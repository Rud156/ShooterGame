using Player.Common;
using System.Collections.Generic;
using UnityEngine;
using Utils.Input;
using Utils.Misc;

namespace Player.Base
{
    [RequireComponent(typeof(CharacterController))]
    public class BasePlayerController : MonoBehaviour
    {
        private const string MOVEMENT_HOLD_IDENTIFIER = "MovementHold";

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

        // Input
        private Vector2 _coreMoveInput;
        private PlayerInputKey _runKey;
        private PlayerInputKey _jumpKey;
        private PlayerInputKey _abilityPrimaryKey;
        private PlayerInputKey _abilitySecondaryKey;
        private PlayerInputKey _abilityTertiaryKey;
        private PlayerInputKey _abilityUltimateKey;
        private PlayerInputKey _constantSpeedFallKey;

        private float _currentStateVelocity;

        // Movement/Controller
        private CharacterController _characterController;
        private List<PlayerState> _playerStateStack;
        private Vector3 _characterVelocity;

        public bool IsGrounded => _isGrounded;
        private bool _isGrounded;

        // Movement Modifiers
        private List<PlayerEffectsAndInputModifiers> _playerEffectsInputsModifiers;

        // Custom Ability
        private Ability _currentAbility;
        private List<PlayerInputRestrictingStoreData> _playerInputRestrictingEffects;

        public delegate void PlayerStatePushed(PlayerState newState);

        public delegate void PlayerStatePopped(PlayerState poppedState);

        public delegate void PlayerGroundedChange(bool previousState, bool newState);

        public delegate void PlayerJumped();

        public delegate void PlayerAbilityStarted(Ability ability);

        public delegate void PlayerAbilityEnded(Ability ability);

        public PlayerStatePushed OnPlayerStatePushed;
        public PlayerStatePopped OnPlayerStatePopped;
        public PlayerGroundedChange OnPlayerGroundedChanged;
        public PlayerJumped OnPlayerJumped;
        public PlayerAbilityStarted OnPlayerAbilityStarted;
        public PlayerAbilityEnded OnPlayerAbilityEnded;

        #region Unity Functions

        private void Start()
        {
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

            PushPlayerState(PlayerState.Idle);
        }

        private void Update() => HandleKeyboardInput();

        private void FixedUpdate()
        {
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
                    }
                }
            }

            if (_playerInputRestrictingEffects.Count <= 0)
            {
                PopPlayerState();
            }
        }

        private PlayerInputRestrictingData GetCustomEffectForState(PlayerInputRestrictingState playerState)
        {
            foreach (var effect in _playerInputRestrictingEffectsData)
            {
                if (effect.targetState == playerState)
                {
                    return effect;
                }
            }

            throw new System.Exception("Invalid State Requested");
        }

        private void SetupPlayerInputRestrictingState()
        {
            var topState = _playerStateStack[^1];
            if (topState != PlayerState.CustomInputRestrictingStates)
            {
                PushPlayerState(PlayerState.CustomInputRestrictingStates);
            }
        }

        private void DestroyPlayerInputCustomEffect(PlayerInputRestrictingStoreData playerCustomEffectOutput) => Destroy(playerCustomEffectOutput.Effect);

        private GameObject SpawnGenericEffectPrefab(PlayerInputRestrictingState stateType)
        {
            var customEffect = GetCustomEffectForState(stateType);
            var spawnPosition = transform.position;
            var forward = transform.forward;
            var right = transform.right;

            spawnPosition += forward * customEffect.effectSpawnOffset.z + right * customEffect.effectSpawnOffset.x;
            spawnPosition.y += customEffect.effectSpawnOffset.y;

            var effect = Instantiate(customEffect.effectPrefab, spawnPosition, Quaternion.identity);
            return effect;
        }

        public void FreezeCharacter(float abilityDuration)
        {
            SetupPlayerInputRestrictingState();

            var effect = SpawnGenericEffectPrefab(PlayerInputRestrictingState.Frozen);
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

            var effect = SpawnGenericEffectPrefab(PlayerInputRestrictingState.Knockback);
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

            var effect = SpawnGenericEffectPrefab(PlayerInputRestrictingState.Stun);
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
                        _playerEffectsInputsModifiers.RemoveAt(i);
                    }
                    else
                    {
                        _playerEffectsInputsModifiers[i] = movementModifier;
                    }
                }
                else if (_playerEffectsInputsModifiers[i].ModifierType == PlayerEffectsAndInputModifierType.ConstantSpeedFall && _isGrounded)
                {
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
                    _playerEffectsInputsModifiers.RemoveAt(i);
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
            // TODO: Trigger with Global UI Handler to implement Paranoia

            _playerEffectsInputsModifiers.Add(
                new PlayerEffectsAndInputModifiers()
                {
                    CurrentDuration = duration,
                    IsTimed = true,
                    ModifierType = PlayerEffectsAndInputModifierType.Paranoia,
                    ModifierIdentifier = string.Empty,
                }
            );
        }

        public void CharacterEnableEngineerShield(float duration)
        {
            // TODO: Add handling for effect and stuff

            _playerEffectsInputsModifiers.Add(
                new PlayerEffectsAndInputModifiers()
                {
                    CurrentDuration = duration,
                    IsTimed = true,
                    ModifierType = PlayerEffectsAndInputModifierType.EngineerShield,
                    ModifierIdentifier = string.Empty,
                }
            );
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
                        ModifierIdentifier = MOVEMENT_HOLD_IDENTIFIER,
                        FloatModifierAmount = _constantSpeedFallMultiplier,
                    }
                );
            }
            else if (_constantSpeedFallKey.KeyReleasedThisFrame || !_constantSpeedFallKey.KeyPressed)
            {
                CheckAndRemovePlayerEffectsAndInputsModifier(MOVEMENT_HOLD_IDENTIFIER);
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
            if (_runKey.KeyPressedThisFrame && _isGrounded)
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
            var forward = transform.forward;
            var right = transform.right;

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

        #endregion Non Movement Abilities

        #region Player State

        private void PushPlayerState(PlayerState state)
        {
            _playerStateStack.Add(state);
            OnPlayerStatePushed?.Invoke(state);
        }

        private void PopPlayerState()
        {
            var topState = _playerStateStack[^1];
            _playerStateStack.RemoveAt(_playerStateStack.Count - 1);
            OnPlayerStatePopped?.Invoke(topState);
        }

        #endregion Player State

        #region Inputs

        private void HandleKeyboardInput()
        {
            _coreMoveInput.x = Input.GetAxisRaw(InputKeys.Horizontal);
            _coreMoveInput.y = Input.GetAxisRaw(InputKeys.Vertical);

            _jumpKey.UpdateInputData(InputKeys.Jump);
            _runKey.UpdateInputData(InputKeys.Run);
            _abilityPrimaryKey.UpdateInputData(InputKeys.AbilityPrimary);
            _abilitySecondaryKey.UpdateInputData(InputKeys.AbilitySecondary);
            _abilityTertiaryKey.UpdateInputData(InputKeys.AbilityTertiary, InputKeys.AbilityTertiarySecondary);
            _abilityUltimateKey.UpdateInputData(InputKeys.AbilityUltimate);
            _constantSpeedFallKey.UpdateInputData(InputKeys.MovementHoldInput);
        }

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
                _ => throw new System.Exception("Invalid Trigger Type"),
            };
        }

        public Vector2 GetCoreMoveInput() => _coreMoveInput;
        public PlayerInputKey GetJumpKey() => _jumpKey;
        public PlayerInputKey GetRunKey() => _runKey;
        public PlayerInputKey GetPrimaryAbilityKey() => _abilityPrimaryKey;
        public PlayerInputKey GetSecondaryAbilityKey() => _abilitySecondaryKey;
        public PlayerInputKey GetTertiaryAbilityKey() => _abilityTertiaryKey;
        public PlayerInputKey GetUltimateAbilityKey() => _abilityUltimateKey;

        #endregion Inputs

        #region Structs

        [System.Serializable]
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
            public string ModifierIdentifier;
            public PlayerEffectsAndInputModifierType ModifierType;
            public bool IsTimed;
            public float CurrentDuration;
            public float FloatModifierAmount;
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