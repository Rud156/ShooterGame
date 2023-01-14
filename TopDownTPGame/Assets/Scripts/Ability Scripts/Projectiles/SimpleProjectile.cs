#region

using UnityEngine;

#endregion

namespace Ability_Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class SimpleProjectile : MonoBehaviour, IProjectile
    {
        [Header("Projectile Data")]
        [SerializeField] private float _projectileLaunchVelocity;
        [SerializeField] private float _projectileDestroyTime;

        private Rigidbody _rb;
        private float _destroyTimeLeft;

        private bool _isInitialized;

        #region Unity Functions

        private void Start() => Init();

        private void OnTriggerEnter(Collider other) => ProjectileHit(other);

        private void FixedUpdate()
        {
            _destroyTimeLeft -= Time.fixedDeltaTime;
            if (_destroyTimeLeft < 0)
            {
                ProjectileDestroy();
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void LaunchProjectile(Vector3 direction)
        {
            Init();

            _rb.velocity = direction * _projectileLaunchVelocity;
            _destroyTimeLeft = _projectileDestroyTime;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        public void ProjectileHit(Collider other) => ProjectileDestroy();

        public void ProjectileDestroy() => Destroy(gameObject);

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
        }

        #endregion Utils
    }
}