#region

using System;
using Player.Base;
using Player.Common;
using UnityEngine;

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

        [SerializeField] private SatchelVelocityState _velocityState;
        [SerializeField] private float _currentTimer;

        private float _currentVelocity;
        private Vector3 _direction;

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
                    _currentTimer -= Time.fixedDeltaTime;
                    if (_currentTimer <= 0)
                    {
                        _currentTimer = _satchelAffectTime;
                        SetSatchelVelocityState(SatchelVelocityState.VelocityDecrease);
                    }
                }
                    break;

                case SatchelVelocityState.VelocityDecrease:
                {
                    var yVelocity = _computedVelocity.y;
                    _currentVelocity -= _velocityDecreaseRate * Time.fixedDeltaTime;
                    if (_currentVelocity < 0)
                    {
                        _currentVelocity = 0;
                    }

                    _computedVelocity = _direction * _currentVelocity;
                    _computedVelocity.y = yVelocity;

                    if (!playerController.IsGrounded)
                    {
                        ProcessGravity();
                    }

                    _currentTimer -= Time.fixedDeltaTime;
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
            _currentVelocity = velocity;

            _currentTimer = _velocityDecreaseRateDelay;
            _direction = direction;

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
            airMovement = _airControlMultiplier * _currentVelocity * airMovement.normalized;

            airMovement.x += _computedVelocity.x;
            airMovement.z += _computedVelocity.z;

            _computedVelocity.x = airMovement.x;
            _computedVelocity.z = airMovement.z;
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