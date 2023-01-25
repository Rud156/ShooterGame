#region

using System.Collections.Generic;
using System.Linq;
using HealthSystem;
using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_3
{
    public class Type_3_Primary_DarkShoot : Ability
    {
        private const int MaxCollidersCheck = 10;

        [Header("Prefabs")]
        [SerializeField] private GameObject _damageEffectPrefab;

        [Header("Components")]
        [SerializeField] private AbilityPrefabInitializer _prefabInit;

        [Header("Shoot Data")]
        [SerializeField] private float _fireRate;
        [SerializeField] private LayerMask _attackMask;
        [SerializeField] private int _damageAmount;
        [SerializeField] private float _raycastDistance;

        [Header("Overheat")]
        [SerializeField] private float _overheatTime;
        [SerializeField] private float _overheatCooldownMultiplier;

        [Header("Post Start Filled")]
        [SerializeField] private Transform _frontCollider;
        [SerializeField] private List<Transform> _raycastPoints;

        private Collider[] _hitColliders = new Collider[MaxCollidersCheck];

        private float _nextShootTime;
        private float _currentOverheatTime;
        private bool _abilityEnd;

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (Time.time >= _nextShootTime)
            {
                _nextShootTime = Time.time + _fireRate;
                _currentOverheatTime += _fireRate;

                if (_currentOverheatTime >= _overheatTime)
                {
                    _currentCooldownDuration = _cooldownDuration;
                    _currentOverheatTime = 0;
                    _abilityEnd = true;
                }

                var totalHitColliders = Physics.OverlapBoxNonAlloc(_frontCollider.position, _frontCollider.localScale / 2, _hitColliders, _frontCollider.rotation, _attackMask);
                for (var i = 0; i < totalHitColliders; i++)
                {
                    // Do not target itself
                    if (_hitColliders[i] == null || _hitColliders[i].gameObject.GetInstanceID() == gameObject.GetInstanceID())
                    {
                        continue;
                    }

                    if (_hitColliders[i].TryGetComponent(out HealthAndDamage healthAndDamage))
                    {
                        var targetHit = false;
                        var targetInstanceId = _hitColliders[i].gameObject.GetInstanceID();

                        foreach (var raycastTarget in _raycastPoints)
                        {
                            var hit = Physics.Raycast(raycastTarget.position, _frontCollider.forward, out var hitInfo, _raycastDistance, _attackMask);
                            if (hit && hitInfo.transform.gameObject.GetInstanceID() == targetInstanceId)
                            {
                                targetHit = true;
                                healthAndDamage.TakeDamage(_damageAmount);
                                Instantiate(_damageEffectPrefab, hitInfo.point, Quaternion.identity);
                                break;
                            }
                        }

                        if (targetHit)
                        {
                            break;
                        }
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

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            base.UnityStartDelegate(playerController);

            _prefabInit.AbilityPrefabInit();
            _frontCollider = transform.Find("CameraHolder/Main Camera/Type_3_CameraPrefab(Clone)/FrontColliderDetector");
            var raycastParent = transform.Find("CameraHolder/Main Camera/Type_3_CameraPrefab(Clone)/RaycastPoints");
            _raycastPoints = raycastParent.GetComponentsInChildren<Transform>().ToList();
        }

        public override void UnityFixedUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityFixedUpdateDelegate(playerController);

            if (_currentOverheatTime > 0 && _abilityEnd)
            {
                _currentOverheatTime -= Time.fixedDeltaTime * _overheatCooldownMultiplier;
            }
        }
    }
}