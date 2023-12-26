using System;
using UnityEngine;

namespace Player.Core
{
    public class PlayerAnimationController : MonoBehaviour
    {
        private static readonly int JumpParam = Animator.StringToHash("Jump");
        private static readonly int FallingParam = Animator.StringToHash("Falling");
        private static readonly int VerticalParam = Animator.StringToHash("Vertical");

        [Header("Components")]
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private PlayerController _playerController;

        #region Unity Functions

        private void Start()
        {
            _playerController.OnPlayerJumped += HandlePlayerJumped;
            _playerController.OnPlayerStateChanged += HandlePlayerStateChanged;
        }

        private void OnDestroy()
        {
            _playerController.OnPlayerJumped -= HandlePlayerJumped;
            _playerController.OnPlayerStateChanged -= HandlePlayerStateChanged;
        }

        #endregion Unity Functions

        #region Animation State Handling

        private void HandlePlayerJumped() => _playerAnimator.SetTrigger(JumpParam);

        private void HandlePlayerStateChanged(PlayerState previousState, PlayerState newState)
        {
            switch (previousState)
            {
                case PlayerState.Idle:
                case PlayerState.Running:
                    _playerAnimator.SetFloat(VerticalParam, 0);
                    break;

                case PlayerState.Falling:
                    _playerAnimator.SetBool(FallingParam, false);
                    break;

                case PlayerState.Dead:
                case PlayerState.CustomMovement:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(previousState), previousState, null);
            }

            switch (newState)
            {
                case PlayerState.Idle:
                    _playerAnimator.SetFloat(VerticalParam, 0);
                    break;

                case PlayerState.Running:
                    _playerAnimator.SetFloat(VerticalParam, 1);
                    break;

                case PlayerState.Falling:
                    _playerAnimator.SetBool(FallingParam, true);
                    break;

                case PlayerState.Dead:
                case PlayerState.CustomMovement:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        #endregion Animation State Handling
    }
}