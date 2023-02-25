#region

using System;
using System.Collections;
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
        private static readonly int IsWalkingParam = Animator.StringToHash("IsWalking");
        private static readonly int IsRunningParam = Animator.StringToHash("IsRunning");

        private const float MaxWalkingAnimValue = 0.5f;
        private const float MaxRunningAnimValue = 1;
        private const float MaxHorizontalAnimValue = 1;

        [Header("Components")]
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private BasePlayerController _playerController;

        [Header("Idle Animations")]
        [SerializeField] private float _delayedIdleStartBuffer;
        [SerializeField] private List<IdleAnimData> _idleAnimDurations;

        // Animator Data
        private Vector2 _movementAnim;

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
            if (_movementAnim.x != 0 || _movementAnim.y != 0)
            {
                _playerAnimator.SetInteger(IdleTriggerParam, 0);
            }
            else
            {
                _playerAnimator.SetBool(IsWalkingParam, false);
                _playerAnimator.SetBool(IsRunningParam, false);
            }

            _playerAnimator.SetFloat(HorizontalParam, _movementAnim.x);
            _playerAnimator.SetFloat(VerticalParam, _movementAnim.y);
        }

        #endregion Anim Updates

        #region Core Movement

        private void HandleCoreMovement()
        {
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
                    SetGroundedAnimValue(MaxWalkingAnimValue);
                    break;

                case PlayerState.Running:
                    SetGroundedAnimValue(MaxRunningAnimValue);
                    break;

                case PlayerState.Falling:
                    SetFallingAnimValue();
                    break;

                case PlayerState.CustomMovement:
                    // Don't handle this...
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandlePlayerStateChanged(PlayerState currentState)
        {
            CheckAndStartNewIdleAnim();

            var playerState = _playerController.GetTopPlayerState();
            switch (playerState)
            {
                case PlayerState.Idle:
                    break;

                case PlayerState.Walking:
                {
                    _playerAnimator.SetBool(IsWalkingParam, true);
                    _playerAnimator.SetBool(IsRunningParam, false);
                }
                    break;

                case PlayerState.Running:
                {
                    _playerAnimator.SetBool(IsWalkingParam, false);
                    _playerAnimator.SetBool(IsRunningParam, true);
                }
                    break;

                case PlayerState.Falling:
                    break;

                case PlayerState.CustomMovement:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetGroundedAnimValue(float maxVerticalValue)
        {
            var coreInput = _playerController.GetCoreMoveInput();
            if (ExtensionFunctions.IsNearlyEqual(coreInput.x, 0))
            {
                _movementAnim.x = 0;
            }
            else
            {
                _movementAnim.x = coreInput.x > 0 ? MaxHorizontalAnimValue : -MaxHorizontalAnimValue;
            }

            if (ExtensionFunctions.IsNearlyEqual(coreInput.y, 0))
            {
                _movementAnim.y = 0;
            }
            else
            {
                _movementAnim.y = coreInput.y > 0 ? maxVerticalValue : -maxVerticalValue;
            }
        }

        private void SetFallingAnimValue()
        {
            var currentValue = _playerAnimator.GetInteger(FallJumpTriggerParam);
            var yVelocity = Mathf.Abs(_playerController.CharacterVelocity.y);

            if ((FallJumpAnimEnums)currentValue == FallJumpAnimEnums.None && yVelocity > _playerController.GroundedVelocityThreshold)
            {
                _playerAnimator.SetInteger(FallJumpTriggerParam, (int)FallJumpAnimEnums.Falling);
            }
        }

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
                _playerAnimator.SetInteger(IdleTriggerParam, 0);

                var randomIndex = GetRandomIdleAnimation();
                StartCoroutine(StartDelayedRandomAnim(randomIndex));
            }
            else
            {
                StartCoroutine(RestartDelayedCurrentAnim());
            }
        }

        private IEnumerator StartDelayedRandomAnim(int randomAnimIndex)
        {
            yield return new WaitForSeconds(_delayedIdleStartBuffer);

            var currentAnimData = _idleAnimDurations[randomAnimIndex];
            _currentAnimIndex = randomAnimIndex;
            _currentIdleAnimTimer = currentAnimData.animDuration;
            _currentIdleAnimPlayCountLeft = Random.Range(currentAnimData.minPlayCount, currentAnimData.maxPlayCount + 1);
            _playerAnimator.SetInteger(IdleTriggerParam, randomAnimIndex + 1);
        }

        private IEnumerator RestartDelayedCurrentAnim()
        {
            yield return new WaitForSeconds(_delayedIdleStartBuffer);

            var currentAnimData = _idleAnimDurations[_currentAnimIndex];
            _currentIdleAnimTimer = currentAnimData.animDuration;
            _playerAnimator.SetInteger(IdleTriggerParam, _currentAnimIndex + 1);
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
            var playerState = _playerController.GetTopPlayerState();
            switch (playerState)
            {
                case PlayerState.Idle:
                case PlayerState.Walking:
                case PlayerState.Falling:
                    _playerAnimator.SetInteger(FallJumpTriggerParam, (int)FallJumpAnimEnums.JumpStandingLaunch);
                    break;

                case PlayerState.Running:
                    _playerAnimator.SetInteger(FallJumpTriggerParam, (int)FallJumpAnimEnums.RunJumpForward);
                    break;
                case PlayerState.CustomMovement:
                    // Don't do anything here...
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleGroundedChanged(bool previousState, bool newState)
        {
            if (newState)
            {
                _playerAnimator.SetInteger(FallJumpTriggerParam, (int)FallJumpAnimEnums.None);
            }
        }

        #endregion Fall/Jump

        #endregion Utils

        #region Structs

        [Serializable]
        private struct IdleAnimData
        {
            public float animDuration;
            public int minPlayCount;
            public int maxPlayCount;
        }

        #endregion Structs

        #region Enums

        private enum FallJumpAnimEnums
        {
            None = 0,
            Falling = 1,
            JumpStandingLaunch = 2,
            RunJumpForward = 3,
        }

        #endregion Enums
    }
}