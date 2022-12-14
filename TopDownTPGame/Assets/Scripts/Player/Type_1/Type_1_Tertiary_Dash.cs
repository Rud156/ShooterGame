using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Misc;

namespace Player.Type_1
{
    public class Type_1_Tertiary_Dash : Ability
    {
        [Header("Dash Data")]
        [SerializeField] private float _dashDuration;
        [SerializeField] private float _dashVelocity;

        private float _currentDashTimeLeft;
        private Vector3 _computedVelocity;

        public override void StartAbility() => _currentDashTimeLeft = _dashDuration;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            Vector3 currentVelocity = playerController.GetCharacterVelocity();
            Vector3 coreInput = playerController.GetCoreMoveInput();
            _currentDashTimeLeft -= Time.fixedDeltaTime;

            // Override X and Z
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // Basically when there is no input use forward only...
            if (ExtensionFunctions.IsNearlyEqual(coreInput.x, 0) && ExtensionFunctions.IsNearlyEqual(coreInput.y, 0))
            {
                coreInput.y = 1;
            }

            _computedVelocity = forward * coreInput.y + right * coreInput.x;
            _computedVelocity = _dashVelocity * _computedVelocity.normalized;
            _computedVelocity.y = currentVelocity.y;
        }

        public override void EndAbility()
        {
        }

        public override bool AbilityCanStart() => true;

        public override bool AbilityNeedsToEnd() => _currentDashTimeLeft <= 0;

        public override Vector3 GetMovementData() => _computedVelocity;
    }
}