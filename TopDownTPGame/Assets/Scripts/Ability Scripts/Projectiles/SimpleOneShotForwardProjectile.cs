using UnityEngine;

namespace AbilityScripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class SimpleOneShotForwardProjectile : MonoBehaviour, IProjectile
    {
        [Header("Prjectile Data")]
        [SerializeField] private float _projectileLaunchVelocity;
        [SerializeField] private float _projectileDestroyTime;

        private Rigidbody _rb;
        private float _currentTimeLeft;
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
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void LaunchProjectile(Vector3 direction)
        {
            Init();

            _isLaunched = true;
            _rb.velocity = direction * _projectileLaunchVelocity;
            _currentTimeLeft = _projectileDestroyTime;
        }

        public void ProjectileHit(Collider other)
        {
        }

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