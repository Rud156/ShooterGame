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

        // Input
        private Vector2 _coreMoveInput;
        private PlayerInputKey _runKey;
        private PlayerInputKey _jumpKey;
        private PlayerInputKey _ability_1_Key;
        private PlayerInputKey _ability_2_Key;
        private float _currentStateVelocity;

        // Movement/Controller
        private CharacterController _characterController;
        private List<PlayerState> _playerStateStack;
        private Vector3 _characterVelocity;
        private bool _isGrounded;

        // Custom Abilities
        private Ability _ability_1;
        private Ability _ability_2;

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
            Ability[] abilityList = GetComponentsInChildren<Ability>();
            _ability_1 = abilityList[0];
            _ability_2 = abilityList[1];

            _coreMoveInput = new Vector2();
            _currentStateVelocity = 0;
            _runKey = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            _jumpKey = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            _ability_1_Key = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            _ability_2_Key = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };

            PushPlayerState(PlayerState.Idle);
        }

        private void Update() => HandleKeyboardInput();

        private void FixedUpdate()
        {
            UpdateGroundedState();
            ProcessJumpInput();
            ProcessCustomMovementInput();
            UpdatePlayerMovement();

            if (_playerStateStack[^1] != PlayerState.Custom)
            {
                ProcessGlobalGravity();
            }
            ApplyFinalMovement();

            ProcessOtherAbilities();

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
            _ability_1.AbilityUpdate(this);
            _characterVelocity = _ability_1.GetMovementData();
            if (_ability_1.AbilityNeedsToEnd())
            {
                _ability_1.EndAbility();
                PopPlayerState();
            }
        }

        #endregion Movement

        #region Core Movement

        private void ProcessCustomMovementInput()
        {
            if (_ability_1_Key.keyPressedThisFrame &&
                _ability_1.AbilityCanStart() &&
                _ability_1.GetAbilityType() == AbilityType.Movement)
            {
                _ability_1.StartAbility();
                PushPlayerState(PlayerState.Custom);
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

        private void ProcessOtherAbilities()
        {
            // TODO: Make sure ability keys are interchangable...
            // DO NOT fix Movement to be Ability 1 etc...
            if (_ability_2_Key.keyPressed)
            {
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
            _ability_1_Key.UpdateInputData(InputKeys.AbilityMovement);
            _ability_2_Key.UpdateInputData(InputKeys.AbilityPrimary);
        }

        private void MarkFrameInputsAsRead()
        {
            _coreMoveInput.x = 0;
            _coreMoveInput.y = 0;

            _jumpKey.ResetPerFrameInput();
            _runKey.ResetPerFrameInput();
            _ability_1_Key.ResetPerFrameInput();
            _ability_2_Key.ResetPerFrameInput();
        }

        private bool HasNoDirectionalInput() => ExtensionFunctions.IsNearlyEqual(_coreMoveInput.x, 0) && ExtensionFunctions.IsNearlyEqual(_coreMoveInput.y, 0);

        public Vector2 GetCoreMoveInput() => _coreMoveInput;

        public PlayerInputKey GetJumpKey() => _jumpKey;

        public PlayerInputKey GetRunKey() => _runKey;

        public PlayerInputKey GetAbilityKey() => _ability_1_Key;

        #endregion Inputs
    }
}