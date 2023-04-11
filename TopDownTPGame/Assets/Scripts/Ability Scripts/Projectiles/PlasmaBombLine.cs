#region

using HealthSystem;
using UnityEngine;

#endregion

namespace Ability_Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlasmaBombLine : MonoBehaviour, IProjectile
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _destroyEffectPrefab;
        [SerializeField] private GameObject _plasmaPulsePrefab;

        [Header("Projectile Data")]
        [SerializeField] private float _projectileLaunchVelocity;
        [SerializeField] private float _projectileDestroyTime;
        [SerializeField] private float _pulseDropRate;

        [Header("Damage Data")]
        [SerializeField] private int _damageAmount;

        private Rigidbody _rb;
        private bool _isInitialized;

        private float _destroyTimeLeft;
        private float _nextBombDropTime;

        #region Unity Functions

        private void Start() => Init();

        private void OnTriggerEnter(Collider other) => ProjectileHit(other);

        private void Update()
        {
            _destroyTimeLeft -= Time.deltaTime;
            if (_destroyTimeLeft < 0)
            {
                ProjectileDestroy();
            }

            if (Time.time >= _nextBombDropTime)
            {
                Instantiate(_plasmaPulsePrefab, transform.position, Quaternion.identity);
                _nextBombDropTime = Time.time + _pulseDropRate;
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void LaunchProjectile(Vector3 direction)
        {
            Init();

            _rb.velocity = direction * _projectileLaunchVelocity;
            _destroyTimeLeft = _projectileDestroyTime;
        }

        public void ProjectileDestroy()
        {
            if (_destroyEffectPrefab)
            {
                Instantiate(_destroyEffectPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }

        public void ProjectileHit(Collider other)
        {
            if (other.TryGetComponent(out HealthAndDamage healthAndDamage))
            {
                healthAndDamage.TakeDamage(_damageAmount);
            }
        }

        #endregion External Functions

        #region Utils

        private void Init()
        {
            if (_isInitialized)
            {
                return;
            }

            _rb = GetComponent<Rigidbody>();
            _isInitialized = true;
            _nextBombDropTime = 0;
        }

        #endregion Utils
    }
}