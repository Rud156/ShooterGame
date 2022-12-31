using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Misc;

namespace Player.Type_3
{
    public class Type_3_Tertiary_ShadowDash : Ability
    {
        [Header("Components")]
        [SerializeField] private Transform _cameraHolder;

        [Header("Dash Charges")]
        [SerializeField] private int _dashCharges;
        [SerializeField] private float _dashDuration;
        [SerializeField] private float _dashVelocity;

        [Header("Dash Float")]
        [SerializeField] private float _dashEndFloatDuration;
        [SerializeField] private float _dashEndFloatFallMultiplier;

        private int _currentDashUsedCount;
        private float _currentDashTimeLeft;
        private Vector3 _computedVelocity;

        public override bool AbilityCanStart(BasePlayerController playerController)
        {
            // TODO: Implement cooldown here...

            return true;
        }

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _currentDashTimeLeft <= 0;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var coreInput = playerController.GetCoreMoveInput();
            _currentDashTimeLeft -= Time.fixedDeltaTime;

            // Basically when there is no input use forward only...
            if (ExtensionFunctions.IsNearlyEqual(coreInput.x, 0) && ExtensionFunctions.IsNearlyEqual(coreInput.y, 0))
            {
                coreInput.y = 1;
            }

            var forward = _cameraHolder.forward;
            var right = _cameraHolder.right;

            _computedVelocity = forward * coreInput.y + right * coreInput.x;
            _computedVelocity = _dashVelocity * _computedVelocity.normalized;
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            playerController.PlayerConstantSpeedFallTimed(_dashEndFloatDuration, _dashEndFloatFallMultiplier);

            _currentDashUsedCount += 1;
            if (_currentDashUsedCount > _dashCharges)
            {
                _currentDashUsedCount = 0;
            }
        }

        public override void StartAbility(BasePlayerController playerController) => _currentDashTimeLeft = _dashDuration;

        public override Vector3 GetMovementData() => _computedVelocity;
    }
}