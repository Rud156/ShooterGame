#region

using UnityEngine;

#endregion

namespace Ability_Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class ShieldDeployProjectile : MonoBehaviour, IProjectile
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _shieldPrefab;

        [Header("Shield Data")]
        [SerializeField] private float _additionalGravity;
        [SerializeField] private float _projectileDestroyTime;
        [SerializeField] private float _launchVelocity;

        private Rigidbody _rb;
        private bool _isInitialized;

        private float _destroyTimeLeft;

        #region Unity Functions

        private void Start() => Init();

        private void OnTriggerEnter(Collider other) => ProjectileHit(other);

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

        #region External Functions

        public void LaunchProjectile(Vector3 direction)
        {
            Init();

            _rb.velocity = direction * _launchVelocity;
            _destroyTimeLeft = _projectileDestroyTime;
        }

        public void ProjectileDestroy() => Destroy(gameObject);

        public void ProjectileHit(Collider other)
        {
            Instantiate(_shieldPrefab, transform.position, Quaternion.identity);
            ProjectileDestroy();
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