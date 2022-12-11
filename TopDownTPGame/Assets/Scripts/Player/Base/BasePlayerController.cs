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

        // Input
        private Vector2 _coreMoveInput;
        private PlayerInputKey _runKey;
        private PlayerInputKey _jumpKey;
        private PlayerInputKey _ability_1_Key;
        private PlayerInputKey _ability_2_Key;
        private PlayerInputKey _ability_3_Key;
        private PlayerInputKey _ultimateKey;
        private float _currentStateVelocity;

        // Movement/Controller
        private CharacterController _characterController;
        private List<PlayerState> _playerStateStack;
        private Vector3 _characterVelocity;
        private bool _isGrounded;

        // Custom Ability
        private ActiveAbilityData _currentAbility;

        public delegate void PlayerStatePushed(PlayerState newState);
        public delegate void PlayerStatePopped(PlayerState poppedState);
        public delegate void PlayerGroundedChange(bool previousState, bool newState);
        public delegate void PlayerJumped();
        public PlayerStatePushed OnPlayerStatePushed;
        public PlayerStatePopped OnPlayerStatePopped;
        public PlayerGroundedChange OnPlayerGroundedChanged;
        public PlayerJumped OnPlayerJumped;

        #region Unity Functions

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
            _playerStateStack = new List<PlayerState>();

            _coreMoveInput = new Vector2();
            _currentStateVelocity = 0;
            _runKey = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            _jumpKey = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            _ability_1_Key = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            _ability_2_Key = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            _ability_3_Key = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            _ultimateKey = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };

            _currentAbility = new ActiveAbilityData();

            PushPlayerState(PlayerState.Idle);
        }

        private void Update() => HandleKeyboardInput();

        private void FixedUpdate()
        {
            UpdateGroundedState();
            ProcessJumpInput();
            CheckAndActivateCustomMovementAbility();
            UpdatePlayerMovement();

            if (_playerStateStack[^1] != PlayerState.Custom)
            {
                ProcessGlobalGravity();
            }

            ApplyFinalMovement();
            CheckAndActivateOtherAbilities();
            UpdateCustomAbilities();
            MarkFrameInputsAsRead();
        }

        #endregion Unity Functions

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

                case PlayerState.Custom:
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
            if (!_currentAbility.IsValid())
            {
                PopPlayerState();
                return;
            }

            _currentAbility.ability.AbilityUpdate(this);
            _characterVelocity = _currentAbility.ability.GetMovementData();
            if (_currentAbility.ability.AbilityNeedsToEnd())
            {
                _currentAbility.ability.EndAbility();
                _currentAbility.Clear();
                PopPlayerState();
            }
        }

        #endregion Movement

        #region Core Movement

        private void CheckAndActivateCustomMovementAbility()
        {
            if (_currentAbility.IsValid())
            {
                return;
            }

            foreach (Ability ability in _playerAbilities)
            {
                PlayerInputKey key = GetKeyForAbilityTrigger(ability.GetAbilityTrigger());

                if (ability.GetAbilityType() == AbilityType.Movement &&
                    ability.AbilityCanStart() &&
                    key.keyPressedThisFrame &&
                    !_currentAbility.IsValid())
                {
                    _currentAbility.ability = ability;
                    _currentAbility.abilityKey = key;
                    _currentAbility.isMovement = true;

                    _currentAbility.ability.StartAbility();
                    PushPlayerState(PlayerState.Custom);
                    break;
                }
            }
        }

        private void UpdateCoreMovement()
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // When Custom Apply Movement Directly
            if (_playerStateStack[^1] == PlayerState.Custom)
            {
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

            if (!isGrounded && _playerStateStack[^1] != PlayerState.Falling && _playerStateStack[^1] != PlayerState.Custom)
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
            if (_currentAbility.IsValid())
            {
                return;
            }

            foreach (Ability ability in _playerAbilities)
            {
                AbilityTrigger abilityTrigger = ability.GetAbilityTrigger();
                PlayerInputKey key = GetKeyForAbilityTrigger(abilityTrigger);

                if (ability.AbilityCanStart() && !_currentAbility.IsValid() && key.keyPressedThisFrame)
                {
                    _currentAbility.ability = ability;
                    _currentAbility.abilityKey = key;
                    _currentAbility.isMovement = false;

                    _currentAbility.ability.StartAbility();
                    break;
                }
            }
        }

        private void UpdateCustomAbilities()
        {
            // This means there is an ability not active OR the ability active is only for Movement
            if (!_currentAbility.IsValid() || _playerStateStack[^1] == PlayerState.Custom)
            {
                return;
            }

            _currentAbility.ability.AbilityUpdate(this);
            if (_currentAbility.ability.AbilityNeedsToEnd())
            {
                _currentAbility.ability.EndAbility();
                _currentAbility.Clear();
            }
        }

        public ActiveAbilityData GetCurrentAbility() => _currentAbility;

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
            _ability_1_Key.UpdateInputData(InputKeys.AbilityMovement);
            _ability_2_Key.UpdateInputData(InputKeys.AbilityPrimary);
            _ability_3_Key.UpdateInputData(InputKeys.AbilityPrimary);
            _ultimateKey.UpdateInputData(InputKeys.AbilityPrimary);
        }

        private void MarkFrameInputsAsRead()
        {
            _coreMoveInput.x = 0;
            _coreMoveInput.y = 0;

            _jumpKey.ResetPerFrameInput();
            _runKey.ResetPerFrameInput();
            _ability_1_Key.ResetPerFrameInput();
            _ability_2_Key.ResetPerFrameInput();
            _ability_3_Key.ResetPerFrameInput();
            _ultimateKey.ResetPerFrameInput();
        }

        private bool HasNoDirectionalInput() => ExtensionFunctions.IsNearlyEqual(_coreMoveInput.x, 0) && ExtensionFunctions.IsNearlyEqual(_coreMoveInput.y, 0);

        private PlayerInputKey GetKeyForAbilityTrigger(AbilityTrigger abilityTrigger)
        {
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                    return _ability_1_Key;

                case AbilityTrigger.Secondary:
                    return _ability_2_Key;

                case AbilityTrigger.Tertiary:
                    return _ability_3_Key;

                case AbilityTrigger.Ultimate:
                    return _ultimateKey;
            }

            throw new System.Exception("Invalid Trigger Type");
        }

        public Vector2 GetCoreMoveInput() => _coreMoveInput;

        public PlayerInputKey GetJumpKey() => _jumpKey;

        public PlayerInputKey GetRunKey() => _runKey;

        public PlayerInputKey GetAbilityKey() => _ability_1_Key;

        #endregion Inputs
    }
}