#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.Misc;
using Random = UnityEngine.Random;

#endregion

namespace Player.Base
{
    public class BasePlayerAnimController : MonoBehaviour
    {
        private static readonly int FallJumpTriggerParam = Animator.StringToHash("FallingAndJumpTrigger");
        private static readonly int IdleTriggerParam = Animator.StringToHash("Idle");
        private static readonly int HorizontalParam = Animator.StringToHash("Horizontal");
        private static readonly int VerticalParam = Animator.StringToHash("Vertical");

        private const float MaxWalkingAnimValue = 0.5f;
        private const float MaxRunningAnimValue = 1;

        [Header("Components")]
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private BasePlayerController _playerController;

        [Header("Idle Animations")]
        [SerializeField] private int _minAnimPlayCountTime;
        [SerializeField] private int _maxAnimPlayCountTime;
        [SerializeField] private List<float> _idleAnimDurations;

        // Animator Data
        private Vector2 _movementAnim;
        private float _idleAnim;
        private int _fallJumpAnim;

        // Idle Anim
        private int _currentAnimIndex;
        private float _currentIdleAnimTimer;
        private int _currentIdleAnimPlayCountLeft;

        #region Unity Functions

        private void Start()
        {
            _playerController.OnPlayerStateChanged += HandlePlayerStateChanged;
            _playerController.OnPlayerJumped += HandlePlayerJumped;
            _playerController.OnPlayerGroundedChanged += HandleGroundedChanged;

            _movementAnim = Vector2.zero;
            _idleAnim = 0;
            _fallJumpAnim = 0;
        }

        private void OnDestroy()
        {
            _playerController.OnPlayerStateChanged -= HandlePlayerStateChanged;
            _playerController.OnPlayerJumped -= HandlePlayerJumped;
            _playerController.OnPlayerGroundedChanged -= HandleGroundedChanged;
        }

        private void Update()
        {
            HandleCoreMovement();
            UpdateIdleAnim();

            UpdateAnimatorCoreStates();
        }

        #endregion Unity Functions

        #region Utils

        #region Anim Updates

        private void UpdateAnimatorCoreStates()
        {
            _playerAnimator.SetFloat(HorizontalParam, _movementAnim.x);
            _playerAnimator.SetFloat(VerticalParam, _movementAnim.y);
            _playerAnimator.SetFloat(IdleTriggerParam, _idleAnim);
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

        private void HandlePlayerStateChanged(PlayerState currentState) => CheckAndStartNewIdleAnim();

        #endregion Core Movement

        #region Idle Anim

        private void UpdateIdleAnim()
        {
            if (_currentIdleAnimTimer <= 0 || _playerController.GetTopPlayerState() != PlayerState.Idle)
            {
                return;
            }

            _currentIdleAnimTimer -= Time.deltaTime;
            if (_currentIdleAnimTimer <= 0)
            {
                _currentIdleAnimPlayCountLeft -= 1;
                CheckAndStartNewIdleAnim();
            }
        }

        private void CheckAndStartNewIdleAnim()
        {
            if (_currentIdleAnimTimer > 0)
            {
                return;
            }

            var currentState = _playerController.GetTopPlayerState();
            if (currentState != PlayerState.Idle)
            {
                return;
            }

            if (_currentIdleAnimPlayCountLeft <= 0)
            {
                var randomIndex = GetRandomIdleAnimation();
                var currentAnimDuration = _idleAnimDurations[randomIndex];

                _currentAnimIndex = randomIndex;
                _currentIdleAnimTimer = currentAnimDuration;
                _currentIdleAnimPlayCountLeft = Random.Range(_minAnimPlayCountTime, _maxAnimPlayCountTime + 1);
                _idleAnim = ExtensionFunctions.Map(randomIndex, 0, _idleAnimDurations.Count - 1, 0, 1);
            }
            else
            {
                var currentAnimDuration = _idleAnimDurations[_currentAnimIndex];
                _currentIdleAnimTimer = currentAnimDuration;
            }
        }

        private int GetRandomIdleAnimation()
        {
            var randomIndex = Random.Range(0, 100);
            randomIndex %= _idleAnimDurations.Count;

            return randomIndex;
        }

        #endregion Idle Anim

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