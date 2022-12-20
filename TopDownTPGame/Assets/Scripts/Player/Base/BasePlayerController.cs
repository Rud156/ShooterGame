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
        private float _currentStateVelocity;

        // Movement/Controller
        private CharacterController _characterController;
        private List<PlayerState> _playerStateStack;
        private Vector3 _characterVelocity;
        public bool IsGrounded => _isGrounded;
        private bool _isGrounded;

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

            _coreMoveInput = new Vector2();
            _currentStateVelocity = 0;
            _runKey = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            _jumpKey = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            _abilityPrimaryKey = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            _abilitySecondaryKey = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            _abilityTertiaryKey = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            _abilityUltimateKey = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };

            PushPlayerState(PlayerState.Idle);
        }

        private void Update() => HandleKeyboardInput();

        private void FixedUpdate()
        {
            UpdateCustomAbilityEffects();

            if ((int)_playerStateStack[^1] < (int)PlayerState.CustomInputRestrictingStates)
            {
                UpdateGroundedState();
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

        #region Custom Ability States

        private void UpdateCustomAbilityEffects()
        {
            if (_playerStateStack[^1] != PlayerState.CustomInputRestrictingStates)
            {
                return;
            }

            for (int i = _playerInputRestrictingEffects.Count - 1; i >= 0; i--)
            {
                var customAbility = _playerInputRestrictingEffects[i];
                customAbility.TickDuration();
                _playerInputRestrictingEffects[i] = customAbility;

                if (_playerInputRestrictingEffects[i].customEffectDuration <= 0)
                {
                    switch (_playerInputRestrictingEffects[i].targetState)
                    {
                        case PlayerInputRestrictingState.Frozen:
                            {
                                UnFreezeCharacter(_playerInputRestrictingEffects[i]);
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

        public void FreezeCharacter(float abilityDuration)
        {
            PlayerState topState = _playerStateStack[^1];
            if (topState != PlayerState.CustomInputRestrictingStates)
            {
                PushPlayerState(PlayerState.CustomInputRestrictingStates);
            }

            PlayerInputRestrictingData customEffect = GetCustomEffectForState(PlayerInputRestrictingState.Frozen);
            Vector3 spawnPosition = transform.position;
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            spawnPosition += forward * customEffect.effectSpawnOffset.z + right * customEffect.effectSpawnOffset.x;
            spawnPosition.y += customEffect.effectSpawnOffset.y;

            GameObject effect = Instantiate(customEffect.effectPrefab, spawnPosition, Quaternion.identity);
            _playerInputRestrictingEffects.Add(new PlayerInputRestrictingStoreData()
            {
                effect = effect,
                targetState = PlayerInputRestrictingState.Frozen,
                customEffectDuration = abilityDuration
            });
        }

        private void UnFreezeCharacter(PlayerInputRestrictingStoreData playerCustomEffectOutput) => Destroy(playerCustomEffectOutput.effect);

        private PlayerInputRestrictingData GetCustomEffectForState(PlayerInputRestrictingState playerState)
        {
            foreach (PlayerInputRestrictingData effect in _playerInputRestrictingEffectsData)
            {
                if (effect.targetState == playerState)
                {
                    return effect;
                }
            }

            throw new System.Exception("Invalid State Requested");
        }

        #endregion Custom Ability States

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
            if (_runKey.keyPressedThisFrame)
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
            if (HasNoDirectionalInput() || _coreMoveInput.y <= 0 || _runKey.keyPressedThisFrame)
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

            foreach (Ability ability in _playerAbilities)
            {
                PlayerInputKey key = GetKeyForAbilityTrigger(ability.GetAbilityTrigger());

                if (ability.GetAbilityType() == AbilityType.Movement &&
                    ability.AbilityCanStart(this) &&
                    key.keyPressedThisFrame &&
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
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // When Custom Apply Movement Directly
            if (_playerStateStack[^1] == PlayerState.CustomMovement)
            {
                // Do nothing here since Custom Movement handles it...
            }
            else if (_playerStateStack[^1] != PlayerState.Falling)
            {
                Vector3 groundedMovement = forward * _coreMoveInput.y + right * _coreMoveInput.x;
                groundedMovement.y = 0;
                groundedMovement = _currentStateVelocity * groundedMovement.normalized;

                _characterVelocity.x = groundedMovement.x;
                _characterVelocity.z = groundedMovement.z;
            }
            else
            {
                Vector3 airMovement = forward * _coreMoveInput.y + right * _coreMoveInput.x;
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
            bool isValidJumpPressed = _jumpKey.keyPressedThisFrame;
            if (!isValidJumpPressed || !_isGrounded)
            {
                return;
            }

            _characterVelocity.y += _jumpVelocity;
            OnPlayerJumped?.Invoke();
        }

        private void UpdateGroundedState()
        {
            bool isGrounded = Physics.Raycast(_groundedCheckPoint.position, Vector3.down, _groundedCheckDistance, _groundedCheckMask);
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
                _characterVelocity.y += Physics.gravity.y * _gravityMultiplier;
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

            foreach (Ability ability in _playerAbilities)
            {
                AbilityTrigger abilityTrigger = ability.GetAbilityTrigger();
                PlayerInputKey key = GetKeyForAbilityTrigger(abilityTrigger);

                if (ability.AbilityCanStart(this) && _currentAbility == null && key.keyPressedThisFrame)
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
            PlayerState topState = _playerStateStack[^1];
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
            _abilityTertiaryKey.UpdateInputData(InputKeys.AbilityTertiary);
            _abilityUltimateKey.UpdateInputData(InputKeys.AbilityUltimate);
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
            public GameObject effect;
            public PlayerInputRestrictingState targetState;
            public float customEffectDuration;

            public void TickDuration() => customEffectDuration -= Time.fixedDeltaTime;
        }

        #endregion Structs
    }
}