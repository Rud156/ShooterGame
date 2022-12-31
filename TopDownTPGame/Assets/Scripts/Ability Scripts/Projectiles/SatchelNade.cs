using UnityEngine;

namespace Ability_Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class SatchelNade : MonoBehaviour, IProjectile
    {
        [Header("Satchel Data")]
        [SerializeField] private float _additionalGravity;
        [SerializeField] private float _satchelLaunchForce;
        [SerializeField] private float _destroyDuration;

        private Rigidbody _rb;
        private bool _isInitialized;

        private float _destroyTimeLeft;

        #region Unity Functions

        private void Start() => Init();

        private void FixedUpdate()
        {
            _destroyTimeLeft -= Time.fixedDeltaTime;
            if (_destroyTimeLeft <= 0)
            {
                ProjectileDestroy();
            }

            _rb.AddForce(Vector3.down * _additionalGravity, ForceMode.Force);
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
            _destroyTimeLeft = _destroyDuration;
        }

        #endregion Utils
    }
}