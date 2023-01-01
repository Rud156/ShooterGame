#region

using UnityEngine;

#endregion

namespace Ability_Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class RocketProjectile : MonoBehaviour, IProjectile
    {
        [Header("Rocket Data")]
        [SerializeField] private float _projectileLaunchVelocity;
        [SerializeField] private float _projectileDestroyTime;

        private Rigidbody _rb;
        private bool _isInitialized;
        private bool _isLaunched;

        private float _destroyTimeLeft;

        #region Unity Functions

        private void Start() => Init();

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

        public void ProjectileDestroy() => Destroy(gameObject);

        public void ProjectileHit(Collider other)
        {
        }

        #region Utils

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