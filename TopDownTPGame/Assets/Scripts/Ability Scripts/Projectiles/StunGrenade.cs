#region

using HealthSystem;
using Player.Base;
using Player.Common;
using Player.Type_5;
using UnityEngine;

#endregion

namespace Ability_Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class StunGrenade : MonoBehaviour, IProjectile
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _miniGrenadePrefab;
        [SerializeField] private GameObject _stunPrefab;
        [SerializeField] private GameObject _destroyEffect;

        [Header("Grenade Data")]
        [SerializeField] private float _additionalGravity;
        [SerializeField] private int _secondaryGrenadeCount;
        [SerializeField] private float _launchVelocity;
        [SerializeField] private float _projectileDestroyTime;
        [SerializeField] private int _damageAmount;

        [Header("Secondary Grenades")]
        [SerializeField] private float _secondaryLaunchVelocity;

        [Header("Stun Data")]
        [SerializeField] private float _stunEffectRadius;
        [SerializeField] private LayerMask _stunMask;

        private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];

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
            var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _stunEffectRadius, _hitColliders, _stunMask);
            for (var i = 0; i < targetsHit; i++)
            {
                if (_hitColliders[i].TryGetComponent(out BasePlayerController targetController))
                {
                    var targetTransform = targetController.transform;
                    var position = targetTransform.position;
                    var stunObject = Instantiate(_stunPrefab, position, Quaternion.identity, targetTransform);
                    var stun = stunObject.GetComponent<Type_5_Secondary_Stun>();
                    targetController.CheckAndAddExternalAbility(stun);
                }

                if (_hitColliders[i].TryGetComponent(out HealthAndDamage healthAndDamage))
                {
                    healthAndDamage.TakeDamage(_damageAmount);
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

            Instantiate(_destroyEffect, transform.position, Quaternion.identity);
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