using UnityEngine;

namespace AbilityScripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlasmaBombLine : MonoBehaviour, IProjectile
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _plasmaPulsePrefab;

        [Header("Prjectile Data")]
        [SerializeField] private float _projectileLaunchVelocity;
        [SerializeField] private float _projectileDestroyTime;
        [SerializeField] private float _pulseDropRate;

        private Rigidbody _rb;
        private bool _isLaunched;
        private float _currentTimeLeft;
        private float _nextDropTime;

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

            if (Time.time >= _nextDropTime)
            {
                Instantiate(_plasmaPulsePrefab, transform.position, Quaternion.identity);
                _nextDropTime = Time.time + _pulseDropRate;
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

        public void ProjectileDestroy()
        {
        }

        public void ProjectileHit(Collider other)
        {
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
            _nextDropTime = 0;
        }

        #endregion Utils
    }
}