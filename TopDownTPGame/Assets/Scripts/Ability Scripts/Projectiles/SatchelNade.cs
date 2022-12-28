using UnityEngine;

namespace AbilityScripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class SatchelNade : MonoBehaviour, IProjectile
    {
        [Header("Satchel Data")]
        [SerializeField] private float _satchelLaunchForce;
        [SerializeField] private float _destroyDuration;

        private bool _isInitialized;
        private Rigidbody _rb;

        private float _currentDestroyDuration;

        #region Unity Functions

        private void Start() => Init();

        private void FixedUpdate()
        {
            _currentDestroyDuration -= Time.fixedDeltaTime;
            if (_currentDestroyDuration <= 0)
            {
                ProjectileDestroy();
            }
        }

        #endregion Unity Functions

        #region Externals Functions

        public void LaunchProjectile(Vector3 direction)
        {
            Init();
            _rb.AddForce(direction * _satchelLaunchForce, ForceMode.Impulse);
        }

        public void ProjectileDestroy() => Destroy(gameObject);

        public void ProjectileHit(Collider other)
        {
        }

        #endregion Externals Functions

        #region Utils

        private void Init()
        {
            if (_isInitialized)
            {
                return;
            }

            _rb = GetComponent<Rigidbody>();
            _isInitialized = true;
            _currentDestroyDuration = _destroyDuration;
        }

        #endregion Utils
    }
}