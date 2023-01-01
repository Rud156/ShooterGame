#region

using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_2
{
    public class Type_2_Ultimate_WaterBubble : Ability
    {
        private const int MAX_COLLIDERS_CHECK = 10;

        [Header("Bubble Data")]
        [SerializeField] private float _abilityCastRadius;
        [SerializeField] private Vector3 _castOffset;
        [SerializeField] private LayerMask _abilityMask;
        [SerializeField] private float _abilityDuration;

        private bool _abilityEnd;

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var castPosition = transform.position;
            var forward = transform.forward;
            var right = transform.right;

            castPosition += forward * _castOffset.z + right * _castOffset.x;
            castPosition.y += _castOffset.y;

            var anyColliderHit = false;

            var hitColliders = new Collider[MAX_COLLIDERS_CHECK];
            var totalHitColliders = Physics.OverlapSphereNonAlloc(castPosition, _abilityCastRadius, hitColliders, _abilityMask);
            for (var i = 0; i < totalHitColliders; i++)
            {
                // Do not target itself
                if (hitColliders[i] == null || hitColliders[i].gameObject.GetInstanceID() == gameObject.GetInstanceID())
                {
                    continue;
                }

                // TODO: Also check team here...
                if (hitColliders[i].TryGetComponent(out BasePlayerController targetController))
                {
                    targetController.FreezeCharacter(_abilityDuration);
                    anyColliderHit = true;
                }
            }

            if (!anyColliderHit)
            {
                // TODO: Put on cooldown...
            }

            _abilityEnd = true;
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;
    }
}