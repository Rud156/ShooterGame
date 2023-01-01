#region

using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Misc;

#endregion

namespace Player.Type_1
{
    public class Type_1_Tertiary_Dash : Ability
    {
        [Header("Dash Data")]
        [SerializeField] private float _dashDuration;
        [SerializeField] private float _dashVelocity;

        private float _currentDashTimeLeft;
        private Vector3 _computedVelocity;

        public override void StartAbility(BasePlayerController playerController) => _currentDashTimeLeft = _dashDuration;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var currentVelocity = playerController.GetCharacterVelocity();
            var coreInput = playerController.GetCoreMoveInput();
            _currentDashTimeLeft -= Time.fixedDeltaTime;

            // Basically when there is no input use forward only...
            if (ExtensionFunctions.IsNearlyEqual(coreInput.x, 0) && ExtensionFunctions.IsNearlyEqual(coreInput.y, 0))
            {
                coreInput.y = 1;
            }

            var forward = transform.forward;
            var right = transform.right;

            // Override X and Z
            _computedVelocity = forward * coreInput.y + right * coreInput.x;
            _computedVelocity = _dashVelocity * _computedVelocity.normalized;
            _computedVelocity.y = currentVelocity.y;
        }

        public override void EndAbility(BasePlayerController playerController)
        {
        }

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _currentDashTimeLeft <= 0;

        public override Vector3 GetMovementData() => _computedVelocity;
    }
}