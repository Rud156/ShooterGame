#region

using System;
using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Common;

#endregion

namespace Player.Type_4
{
    public class Type_4_Tertiary_SatchelMovement : Ability
    {
        [Header("Satchel Data")]
        [SerializeField] private float _gravityMultiplier;
        [SerializeField] private float _airControlMultiplier;
        [SerializeField] private float _velocityDecreaseRateDelay;
        [SerializeField] private float _velocityDecreaseRate;
        [SerializeField] private float _satchelAffectTime;

        private SatchelVelocityState _velocityState;
        private float _currentTimer;

        private float _maxStartVelocity;
        private Vector3 _computedVelocity;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _velocityState == SatchelVelocityState.EndSatchel;


        public override void AbilityUpdate(BasePlayerController playerController)
        {
            switch (_velocityState)
            {
                case SatchelVelocityState.FullVelocity:
                {
                    _currentTimer -= GlobalStaticData.FixedUpdateTime;
                    if (_currentTimer <= 0)
                    {
                        _currentTimer = _satchelAffectTime;
                        SetSatchelVelocityState(SatchelVelocityState.VelocityDecrease);
                    }
                }
                    break;

                case SatchelVelocityState.VelocityDecrease:
                {
                    DecreaseVelocity();
                    if (!playerController.IsGrounded)
                    {
                        ProcessGravity();
                    }
                    else
                    {
                        _computedVelocity.y = 0;
                    }

                    _currentTimer -= GlobalStaticData.FixedUpdateTime;
                    if (_currentTimer <= 0 || playerController.IsGrounded)
                    {
                        SetSatchelVelocityState(SatchelVelocityState.EndSatchel);
                    }
                }
                    break;

                case SatchelVelocityState.EndSatchel:
                    // Do Nothing Here...
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            ApplyCoreMovement(playerController);
        }

        public override void EndAbility(BasePlayerController playerController) => Destroy(gameObject);

        public override void StartAbility(BasePlayerController playerController)
        {
        }

        #endregion Ability Functions

        #region Specific Data

        public override Vector3 GetMovementData() => _computedVelocity;

        #endregion Specific Data

        #region External Functions

        public void ApplySatchelMovement(Vector3 direction, float velocity)
        {
            _computedVelocity = direction * velocity;
            _maxStartVelocity = velocity;

            _currentTimer = _velocityDecreaseRateDelay;

            SetSatchelVelocityState(SatchelVelocityState.FullVelocity);
        }

        #endregion External Functions

        #region Utils

        private void ProcessGravity() => _computedVelocity.y += Physics.gravity.y * _gravityMultiplier;

        private void ApplyCoreMovement(BasePlayerController playerController)
        {
            var playerTransform = playerController.transform;
            var forward = playerTransform.forward;
            var right = playerTransform.right;
            var coreInput = playerController.GetCoreMoveInput();

            var airMovement = forward * coreInput.y + right * coreInput.x;
            airMovement.y = 0;
            airMovement = airMovement.normalized * (_airControlMultiplier * _maxStartVelocity);

            var clampedXVelocity = Mathf.Clamp(_computedVelocity.x + airMovement.x, -_maxStartVelocity, _maxStartVelocity);
            var clampedZVelocity = Mathf.Clamp(_computedVelocity.z + airMovement.z, -_maxStartVelocity, _maxStartVelocity);

            _computedVelocity.x = clampedXVelocity;
            _computedVelocity.z = clampedZVelocity;
        }

        private void DecreaseVelocity()
        {
            var xVelocity = _computedVelocity.x;
            if (xVelocity < 0)
            {
                xVelocity += _velocityDecreaseRate * GlobalStaticData.FixedUpdateTime;
            }
            else
            {
                xVelocity -= _velocityDecreaseRate * GlobalStaticData.FixedUpdateTime;
            }

            var zVelocity = _computedVelocity.z;
            if (zVelocity < 0)
            {
                zVelocity += _velocityDecreaseRate * GlobalStaticData.FixedUpdateTime;
            }
            else
            {
                zVelocity -= _velocityDecreaseRate * GlobalStaticData.FixedUpdateTime;
            }

            _computedVelocity.x = xVelocity;
            _computedVelocity.z = zVelocity;
        }

        private void SetSatchelVelocityState(SatchelVelocityState velocityState) => _velocityState = velocityState;

        #endregion Utils

        #region Enums

        private enum SatchelVelocityState
        {
            FullVelocity,
            VelocityDecrease,
            EndSatchel,
        }

        #endregion Enums
    }
}