using Player.Base;
using Player.Common;
using System.Collections.Generic;
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

        private List<BasePlayerController> _targets;
        private float _currentDuration;

        #region Unity Functions

        private void Start() => _targets = new List<BasePlayerController>();

        #endregion Unity Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => _targets.Count <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _currentDuration <= 0;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentDuration -= Time.fixedDeltaTime;

            if (_currentDuration <= 0)
            {
                for (int i = 0; i < _targets.Count; i++)
                {
                    _targets[i].UnFreezeCharacter();
                }

                _targets.Clear();
            }
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            throw new System.NotImplementedException();
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
            Physics.OverlapSphereNonAlloc(castPosition, _abilityCastRadius, hitColliders, _abilityMask);
            for (int i = 0; i < MAX_COLLIDERS_CHECK; i++)
            {
                BasePlayerController targetController = hitColliders[i].GetComponent<BasePlayerController>();
                // TODO: Also check team here...
                if (targetController != null)
                {
                    targetController.FreezeCharacter();
                    anyColliderHit = true;

                    _targets.Add(targetController);
                }
            }

            if (anyColliderHit)
            {
                _currentDuration = _abilityDuration;
            }
            else
            {
                // TODO: Put on cooldown...
            }
        }
    }
}