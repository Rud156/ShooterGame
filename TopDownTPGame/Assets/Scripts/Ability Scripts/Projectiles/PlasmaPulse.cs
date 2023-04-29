#region

using System;
using HealthSystem;
using Player.Common;
using Player.Type_4;
using UnityEngine;

#endregion

namespace Ability_Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlasmaPulse : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _destroyEffectPrefab;

        [Header("Components")]
        [SerializeField] private Rigidbody _projectileRb;

        [Header("Bomb Movement")]
        [SerializeField] private float _downwardsLaunchVelocity;

        [Header("Bomb Data")]
        [SerializeField] private float _destroyTime;
        [SerializeField] private float _bombDamageRadius;
        [SerializeField] private int _damageAmount;
        [SerializeField] private LayerMask _bombMask;

        private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];

        private float _destroyTimeLeft;

        private Action<Collider, Type_4_Primary_PlasmaBomb.PlasmaBombType> _callbackFunc;

        #region Unity Functions

        private void Start()
        {
            _destroyTimeLeft = _destroyTime;
            _projectileRb.velocity = _downwardsLaunchVelocity * Vector3.down;
        }

        private void Update()
        {
            _destroyTimeLeft -= Time.deltaTime;
            if (_destroyTimeLeft <= 0)
            {
                DestroyProjectile();
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void SetCollisionCallback(Action<Collider, Type_4_Primary_PlasmaBomb.PlasmaBombType> callback) => _callbackFunc = callback;

        #endregion External Functions

        #region Utils

        private void DestroyProjectile()
        {
            if (_destroyEffectPrefab != null)
            {
                Instantiate(_destroyEffectPrefab, transform.position, Quaternion.identity);
            }

            var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _bombDamageRadius, _hitColliders, _bombMask);
            for (var i = 0; i < targetsHit; i++)
            {
                if (_hitColliders[i] == null)
                {
                    continue;
                }

                if (_hitColliders[i].TryGetComponent(out HealthAndDamage healthAndDamage))
                {
                    healthAndDamage.TakeDamage(_damageAmount);
                    _callbackFunc?.Invoke(_hitColliders[i], Type_4_Primary_PlasmaBomb.PlasmaBombType.PlasmaPulse);
                }
            }

            Destroy(gameObject);
        }

        #endregion Utils
    }
}