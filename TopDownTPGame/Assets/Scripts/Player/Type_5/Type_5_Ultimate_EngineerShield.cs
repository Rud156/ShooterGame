#region

using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_5
{
    public class Type_5_Ultimate_EngineerShield : Ability
    {
        [Header("Shield Data")]
        [SerializeField] private float _shieldDuration;

        [Header("Shield Collision")]
        [SerializeField] private float _shieldColliderRadius;
        [SerializeField] private LayerMask _shieldColliderMask;

        private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];
        private float _currentTimeLeft;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _currentTimeLeft <= 0;

        public override void StartAbility(BasePlayerController playerController) => _currentTimeLeft = _shieldDuration;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentTimeLeft -= Time.fixedDeltaTime;

            var hitColliderCount = Physics.OverlapSphereNonAlloc(transform.position, _shieldColliderRadius, _hitColliders, _shieldColliderMask);
            for (var i = 0; i < hitColliderCount; i++)
            {
                if (_hitColliders[i] == null)
                {
                    continue;
                }

                // TODO: Need to make a common OwnerID script from which to get OwnerID...
                Destroy(_hitColliders[i].gameObject);
            }
        }

        public override void EndAbility(BasePlayerController playerController) => Destroy(gameObject);

        #endregion Ability Functions
    }
}