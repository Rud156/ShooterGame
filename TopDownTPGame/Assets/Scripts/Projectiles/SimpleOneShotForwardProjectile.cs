using UnityEngine;

namespace Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class SimpleOneShotForwardProjectile : MonoBehaviour, IProjectile
    {
        [Header("Prjectile Data")]
        [SerializeField] private float projectileLaunchVelocity;
        [SerializeField] private float projectileDestroyTime;

        private float _currentTimeLeft;
        private Rigidbody _rb;
        private bool _isLaunched;

        private bool _isInitialized = false;

        #region Unity Functions

        private void Start() => Init();

        private void FixedUpdate()
        {
            if (!_isLaunched)
            {
                return;
            }

            _currentTimeLeft -= Time.fixedDeltaTime;
            if (_currentTimeLeft < 0)
            {
                ProjectileDestroy();
                Destroy(gameObject);
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void LaunchProjectile(Vector3 direction)
        {
            Init();

            _isLaunched = true;
            _rb.velocity = direction * projectileLaunchVelocity;
            _currentTimeLeft = projectileDestroyTime;
        }

        public void ProjectileHit(Collider other)
        {
            // TODO: Call damage function here...
        }

        public void ProjectileDestroy()
        {
            // TODO: Call anything here...
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
        }

        #endregion Utils
    }
}