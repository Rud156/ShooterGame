#region

using UnityEngine;

#endregion

namespace Ability_Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlasmaHomingMissile : MonoBehaviour, IProjectile
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _destroyEffect;

        [Header("Homing Data")]
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private float _projectileVelocity;
        [SerializeField] private float _projectileDestroyTime;

        private Rigidbody _rb;

        private Transform _target;
        private float _destroyTimeLeft;

        #region Unity Functions

        private void Start() => _rb = GetComponent<Rigidbody>();

        private void OnTriggerEnter(Collider other) => ProjectileHit(other);

        private void FixedUpdate()
        {
            var targetDirection = _target.position - transform.position;
            targetDirection.Normalize();

            var forward = transform.forward;
            var rotateAmount = -Vector3.Cross(targetDirection, forward);
            _rb.angularVelocity = rotateAmount * _rotationSpeed;
            _rb.velocity = forward * _projectileVelocity;

            _destroyTimeLeft -= Time.fixedDeltaTime;
            if (_destroyTimeLeft <= 0)
            {
                ProjectileDestroy();
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void SetTarget(Transform target)
        {
            _target = target;
            _destroyTimeLeft = _projectileDestroyTime;
        }

        public void LaunchProjectile(Vector3 direction)
        {
        }

        public void ProjectileDestroy()
        {
            Instantiate(_destroyEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        public void ProjectileHit(Collider other) => ProjectileDestroy();

        #endregion External Functions
    }
}