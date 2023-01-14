#region

using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_3
{
    public class Type_3_Primary_DarkShoot : Ability
    {
        private const int MaxColliders = 10;

        [Header("Components")]
        [SerializeField] private AbilityPrefabInitializer _prefabInit;

        [Header("Shoot Data")]
        [SerializeField] private float _fireRate;
        [SerializeField] private LayerMask _attackMask;

        [Header("Post Start Filled")]
        [SerializeField] private Transform _frontCollider;

        private float _nextShootTime;
        private bool _abilityEnd;

        #region Unity Functions

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            base.UnityStartDelegate(playerController);

            _prefabInit.AbilityPrefabInit();
            _frontCollider = transform.Find("CameraHolder/Type_3_CameraPrefab(Clone)/FrontColliderDetector");
        }

        #endregion Unity Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (Time.time >= _nextShootTime)
            {
                _nextShootTime = Time.time + _fireRate;

                var hitColliders = new Collider[MaxColliders];
                var totalHitColliders = Physics.OverlapBoxNonAlloc(_frontCollider.position, _frontCollider.localScale / 2, hitColliders, _frontCollider.rotation, _attackMask);
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
                        // TODO: Implement damage here...

                        // Only hit one target...
                        break;
                    }
                }
            }

            var inputKey = playerController.GetPrimaryAbilityKey();
            if (inputKey.KeyReleasedThisFrame || !inputKey.KeyPressed)
            {
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;
    }
}