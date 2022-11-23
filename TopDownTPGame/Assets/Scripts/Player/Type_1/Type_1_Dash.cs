using Player.Common;
using UnityEngine;
using Utils.Misc;

namespace Player.Type_1
{
    public class Type_1_Dash : AbilityMovement
    {
        [SerializeField] private float _dashDuration;
        [SerializeField] private float _dashVelocity;

        private float _currentDashTimeLeft;

        public override void StartAbility() => _currentDashTimeLeft = _dashDuration;

        public override Vector3 AbilityMove(Vector3 currentVelocity, Vector3 coreInput)
        {
            _currentDashTimeLeft -= Time.fixedDeltaTime;

            // Override X and Z
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // Basically when there is no input use forward only...
            if (ExtensionFunctions.IsNearlyEqual(coreInput.x, 0) && ExtensionFunctions.IsNearlyEqual(coreInput.y, 0))
            {
                coreInput.y = 1;
            }

            Vector3 computedVelocity = forward * coreInput.y + right * coreInput.x;
            computedVelocity = _dashVelocity * computedVelocity.normalized;
            computedVelocity.y = currentVelocity.y;

            return computedVelocity;
        }

        public override void EndAbility()
        {
        }

        public override bool AbilityCanStart() => true;

        public override bool AbilityNeedsToEnd() => _currentDashTimeLeft <= 0;
    }
}