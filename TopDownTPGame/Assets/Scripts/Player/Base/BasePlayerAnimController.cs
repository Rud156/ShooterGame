#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Player.Base
{
    public class BasePlayerAnimController : MonoBehaviour
    {
        private static readonly int FallJumpTrigger = Animator.StringToHash("FallingAndJumpTrigger");
        private static readonly int IdleTrigger = Animator.StringToHash("Idle");
        private static readonly int HorizontalParam = Animator.StringToHash("Horizontal");
        private static readonly int VerticalParam = Animator.StringToHash("Vertical");

        private const float MaxWalkingAnimValue = 0.5f;
        private const float MaxRunningAnimValue = 1;

        [Header("Components")]
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private BasePlayerController _playerController;

        [Header("Idle Animations")]
        [SerializeField] private List<float> _idleAnimDurations;

        private Vector2 _movementAnim;
        private int _idleAnim;
        private int _fallJumpAnim;

        #region Unity Functions

        private void Start()
        {
            _playerController.OnPlayerJumped += HandlePlayerJumped;
            _playerController.OnPlayerGroundedChanged += HandleGroundedChanged;

            _movementAnim = Vector2.zero;
            _idleAnim = 0;
            _fallJumpAnim = 0;
        }

        private void OnDestroy()
        {
            _playerController.OnPlayerJumped -= HandlePlayerJumped;
            _playerController.OnPlayerGroundedChanged -= HandleGroundedChanged;
        }

        private void Update()
        {
            HandleCoreMovement();
            UpdateAnimatorCoreStates();
        }

        #endregion Unity Functions

        #region Utils

        #region Anim Updates

        private void UpdateAnimatorCoreStates()
        {
            _playerAnimator.SetFloat(HorizontalParam, _movementAnim.x);
            _playerAnimator.SetFloat(VerticalParam, _movementAnim.y);
        }

        #endregion Anim Updates

        #region Core Movement

        private void HandleCoreMovement()
        {
            var coreInput = _playerController.GetCoreMoveInput();
            var playerState = _playerController.GetTopPlayerState();

            switch (playerState)
            {
                case PlayerState.Idle:
                {
                    _movementAnim.x = 0;
                    _movementAnim.y = 0;
                }
                    break;

                case PlayerState.Walking:
                {
                    _movementAnim.x = Mathf.Clamp(coreInput.x, -MaxWalkingAnimValue, MaxWalkingAnimValue);
                    _movementAnim.y = Mathf.Clamp(coreInput.y, -MaxWalkingAnimValue, MaxWalkingAnimValue);
                }
                    break;

                case PlayerState.Running:
                {
                    _movementAnim.x = Mathf.Clamp(coreInput.x, -MaxRunningAnimValue, MaxRunningAnimValue);
                    _movementAnim.y = Mathf.Clamp(coreInput.y, -MaxRunningAnimValue, MaxRunningAnimValue);
                }
                    break;

                case PlayerState.Falling:
                    break;

                case PlayerState.CustomMovement:
                    // Don't handle this...
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion Core Movement

        #region Fall/Jump

        private void HandlePlayerJumped()
        {
        }

        private void HandleGroundedChanged(bool previousState, bool newState)
        {
        }

        #endregion Fall/Jump

        #endregion Utils
    }
}