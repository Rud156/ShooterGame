#region

using Player.Base;
using UnityEngine;
using Utils.Misc;

#endregion

namespace Ability_Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class StunGrenade : MonoBehaviour, IProjectile
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _miniGrenadePrefab;

        [Header("Grenade Data")]
        [SerializeField] private float _additionalGravity;
        [SerializeField] private int _secondaryGrenadeCount;
        [SerializeField] private float _launchVelocity;
        [SerializeField] private float _projectileDestroyTime;

        [Header("Secondary Grenades")]
        [SerializeField] private float _secondaryLaunchVelocity;

        [Header("Stun Data")]
        [SerializeField] private float _stunEffectRadius;
        [SerializeField] private float _stunDuration;
        [SerializeField] private LayerMask _stunMask;

        private Collider[] _hitColliders = new Collider[StaticData.MaxCollidersCheck];

        private Rigidbody _rb;
        private bool _isInitialized;

        private bool _isSecondary;
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

        #region External Functions

        public void LaunchProjectile(Vector3 direction)
        {
            Init();
            _rb.velocity = direction * (_isSecondary ? _secondaryLaunchVelocity : _launchVelocity);
            _destroyTimeLeft = _projectileDestroyTime;
        }

        public void ProjectileDestroy()
        {
            // TODO: Add Stun effect to players

            var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _stunEffectRadius, _hitColliders, _stunMask);

            for (var i = 0; i < targetsHit; i++)
            {
                // Do not target itself
                if (_hitColliders[i] == null)
                {
                    continue;
                }

                // TODO: Also check team here...
                if (_hitColliders[i].TryGetComponent(out BasePlayerController targetController))
                {
                    // TODO: Add Stun Ability Script
                }
            }

            if (!_isSecondary)
            {
                var angleDifference = 360.0f / _secondaryGrenadeCount;
                float startAngle = 0;

                for (var i = 0; i < _secondaryGrenadeCount; i++)
                {
                    var secondaryProjectile = Instantiate(_miniGrenadePrefab, transform.position, Quaternion.Euler(0, startAngle, 0));
                    var forward = secondaryProjectile.transform.forward;

                    var stunGrenade = secondaryProjectile.GetComponent<StunGrenade>();
                    stunGrenade.LaunchProjectile(forward);
                    stunGrenade.SetSecondary(true);

                    startAngle += angleDifference;
                }
            }

            Destroy(gameObject);
        }

        public void ProjectileHit(Collider other)
        {
        }

        public void SetSecondary(bool isSecondary) => _isSecondary = isSecondary;

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