#region

using HealthSystem;
using UnityEngine;

#endregion

namespace Ability_Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlasmaBomb : MonoBehaviour
    {
        private const int MaxCollidersCheck = 10;

        [Header("Prefabs")]
        [SerializeField] private GameObject _destroyEffectPrefab;

        [Header("Bomb Data")]
        [SerializeField] private float _destroyTime;
        [SerializeField] private float _bombDamageRadius;
        [SerializeField] private int _damageAmount;
        [SerializeField] private LayerMask _bombMask;

        private Collider[] _hitColliders = new Collider[MaxCollidersCheck];

        private float _destroyTimeLeft;

        #region Unity Functions

        private void Start() => _destroyTimeLeft = _destroyTime;

        private void FixedUpdate()
        {
            _destroyTimeLeft -= Time.fixedDeltaTime;
            if (_destroyTimeLeft <= 0)
            {
                DestroyProjectile();
            }
        }

        #endregion Unity Functions

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
                }
            }

            Destroy(gameObject);
        }

        #endregion Utils
    }
}