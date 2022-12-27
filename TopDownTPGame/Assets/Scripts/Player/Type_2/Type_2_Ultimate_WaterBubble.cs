using Player.Base;
using Player.Common;
using UnityEngine;

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

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => true;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
        }

        public override void EndAbility(BasePlayerController playerController)
        {
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            Vector3 castPosition = transform.position;
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            castPosition += forward * _castOffset.z + right * _castOffset.x;
            castPosition.y += _castOffset.y;

            bool anyColliderHit = false;

            Collider[] hitColliders = new Collider[MAX_COLLIDERS_CHECK];
            int totalHitColliders = Physics.OverlapSphereNonAlloc(castPosition, _abilityCastRadius, hitColliders, _abilityMask);
            for (int i = 0; i < totalHitColliders; i++)
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
        }
    }
}