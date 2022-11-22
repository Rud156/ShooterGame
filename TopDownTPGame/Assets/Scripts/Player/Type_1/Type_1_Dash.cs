using Player.Common;
using UnityEngine;

namespace Player.Type_1
{
    public class Type_1_Dash : AbilityMovement
    {
        [SerializeField] private float _dashDuration;
        [SerializeField] private float _dashVelocity;

        private float _currentDashTimeLeft;

        public override void StartAbility()
        {
            _currentDashTimeLeft = _dashDuration;
        }

        public override Vector3 AbilityMove(Vector3 currentVelocity, Vector3 coreInput)
        {
            // Override X and Z
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            Vector3 computedVelocity = forward * coreInput.y + right * coreInput.x;
            computedVelocity.y = 0;
            computedVelocity = _dashVelocity * computedVelocity.normalized;

            return computedVelocity;
        }

        public override void EndAbility()
        {

        }
    }
}