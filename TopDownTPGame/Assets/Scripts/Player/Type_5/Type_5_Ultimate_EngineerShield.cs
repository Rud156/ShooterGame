#region

using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Common;
using World;

#endregion

namespace Player.Type_5
{
    public class Type_5_Ultimate_EngineerShield : Ability
    {
        [Header("Components")]
        [SerializeField] private OwnerData _ownerData;

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
            _currentTimeLeft -= WorldTimeManager.Instance.FixedUpdateTime;

            var hitColliderCount = Physics.OverlapSphereNonAlloc(transform.position, _shieldColliderRadius, _hitColliders, _shieldColliderMask);
            for (var i = 0; i < hitColliderCount; i++)
            {
                var ownerId = _ownerData.OwnerId;
                var ownerData = _hitColliders[i].GetComponent<OwnerData>();
                var projectileOwnerId = ownerData.OwnerId;

                if (ownerId != projectileOwnerId)
                {
                    Destroy(_hitColliders[i].gameObject);
                }
            }
        }

        public override void EndAbility(BasePlayerController playerController) => Destroy(gameObject);

        #endregion Ability Functions
    }
}