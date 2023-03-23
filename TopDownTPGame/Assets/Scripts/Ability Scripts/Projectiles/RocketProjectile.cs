#region

using HealthSystem;
using UnityEngine;

#endregion

namespace Ability_Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class RocketProjectile : MonoBehaviour, IProjectile
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _destroyEffect;

        [Header("Rocket Data")]
        [SerializeField] private float _projectileLaunchVelocity;
        [SerializeField] private float _projectileDestroyTime;

        [Header("Damage Data")]
        [SerializeField] private float _damageRadius;
        [SerializeField] private int _damageAmount;
        [SerializeField] private LayerMask _rocketMask;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private Rigidbody _rb;
        private bool _isInitialized;
        private bool _isLaunched;

        private float _destroyTimeLeft;

        #region Unity Functions

        private void Start() => Init();

        private void OnTriggerEnter(Collider other) => ProjectileHit(other);

        private void FixedUpdate()
        {
            if (!_isLaunched)
            {
                return;
            }

            _destroyTimeLeft -= Time.fixedDeltaTime;
            if (_destroyTimeLeft < 0)
            {
                ProjectileDestroy();
            }
        }

        #endregion Unity Functions

        public void LaunchProjectile(Vector3 direction)
        {
            _isLaunched = true;
            _rb.velocity = direction * _projectileLaunchVelocity;
            _destroyTimeLeft = _projectileDestroyTime;
        }

        public void ProjectileDestroy()
        {
            ApplyDamageForRocket();
            Instantiate(_destroyEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        public void ProjectileHit(Collider other)
        {
            if (!_isLaunched)
            {
                return;
            }

            ProjectileDestroy();
        }

        #region Utils

        private void ApplyDamageForRocket()
        {
            var targetsHit = Physics.OverlapSphere(transform.position, _damageRadius, _rocketMask);
            if (_debugIsActive)
            {
                DebugExtension.DebugWireSphere(transform.position, Color.red, _damageRadius, _debugDisplayDuration);
            }

            foreach (var target in targetsHit)
            {
                if (target.TryGetComponent(out HealthAndDamage healthAndDamage))
                {
                    healthAndDamage.TakeDamage(_damageAmount);
                }
            }
        }

        private void Init()
        {
            if (_isInitialized)
            {
                return;
            }

            _rb = GetComponent<Rigidbody>();
            _isInitialized = true;
        }

        #endregion Utils
    }
}