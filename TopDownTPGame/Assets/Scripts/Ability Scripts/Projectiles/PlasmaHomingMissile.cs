using UnityEngine;

namespace AbilityScripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlasmaHomingMissile : MonoBehaviour, IProjectile
    {
        [Header("Homing Data")]
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private float _projectileVelocity;
        [SerializeField] private float _projectileDestroyTime;

        private Rigidbody _rb;

        private Transform _target;
        private float _currentDestroyTime;

        #region Unity Functions

        private void Start() => _rb = GetComponent<Rigidbody>();

        private void FixedUpdate()
        {
            Vector3 targetDirection = _target.position - transform.position;
            targetDirection.Normalize();

            Vector3 rotateAmount = -Vector3.Cross(targetDirection, transform.forward);
            _rb.angularVelocity = rotateAmount * _rotationSpeed;
            _rb.velocity = transform.forward * _projectileVelocity;

            _currentDestroyTime -= Time.fixedDeltaTime;
            if (_currentDestroyTime <= 0)
            {
                ProjectileDestroy();
                Destroy(gameObject);
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void SetTarget(Transform target)
        {
            _target = target;
            _currentDestroyTime = _projectileDestroyTime;
        }

        public void LaunchProjectile(Vector3 direction)
        {
        }

        public void ProjectileDestroy()
        {
        }

        public void ProjectileHit(Collider other)
        {
        }

        #endregion External Functions
    }
}