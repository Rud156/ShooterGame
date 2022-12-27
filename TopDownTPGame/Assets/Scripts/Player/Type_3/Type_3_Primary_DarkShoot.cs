using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Input;

namespace Player.Type_3
{
    public class Type_3_Primary_DarkShoot : Ability
    {
        private const int MAX_COLLIDERS = 10;

        [Header("Components")]
        [SerializeField] private Transform _cameraHolder;
        [SerializeField] private AbilityPrefabInitializer _prefabInit;

        [Header("Shoot Data")]
        [SerializeField] private float _fireRate;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private LayerMask _attackMask;

        [Header("Post Start Filled")]
        [SerializeField] private Transform _frontCollider;

        private float _nextFireTime;
        private bool _abilityEnd;

        #region Unity Functions

        private void Start()
        {
            _prefabInit.AbilityPrefabInit();
            _frontCollider = transform.Find("Type_3_Prefab(Clone)/FrontColliderDetector");
        }

        #endregion Unity Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (Time.time >= _nextFireTime)
            {
                _nextFireTime = Time.time + _fireRate;

                Collider[] hitColliders = new Collider[MAX_COLLIDERS];
                int totalHitColliders = Physics.OverlapBoxNonAlloc(_frontCollider.position, _frontCollider.localScale / 2, hitColliders, _frontCollider.rotation, _attackMask);
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
                        // TODO: Implement damage here...
                    }
                }
            }

            PlayerInputKey inputKey = playerController.GetPrimaryAbilityKey();
            if (inputKey.keyReleasedThisFrame || !inputKey.keyPressed)
            {
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;
    }
}