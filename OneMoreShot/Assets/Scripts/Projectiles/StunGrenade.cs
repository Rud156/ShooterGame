using Assets.Scripts.Player.CommonAbilities;
using CustomCamera;
using HeallthSystem;
using Player.Core;
using Player.Misc;
using UnityEngine;
using Utils.Common;

namespace Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class StunGrenade : MonoBehaviour, IProjectile
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _stunPrefab;
        [SerializeField] private GameObject _destroyEffect;

        [Header("Components")]
        [SerializeField] private OwnerData _ownerData;

        [Header("General Grenade Data")]
        [SerializeField] private float _effectRadius;
        [SerializeField] private float _additionalGravity;
        [SerializeField] private float _launchVelocity;
        [SerializeField] private float _projectileDestroyTime;

        [Header("Damage Data")]
        [SerializeField] private int _damageAmount;
        [SerializeField] private LayerMask _damageMask;

        [Header("Secondary Grenades")]
        [SerializeField] private bool _hasSecondaryGrenades;
        [SerializeField] private int _secondaryGrenadeCount;
        [SerializeField] private GameObject _secondaryGrenadePrefab;

        [Header("Stun Data")]
        [SerializeField] private LayerMask _stunMask;

        [Header("Camera Data")]
        [SerializeField] private CameraShakerInRange _grenadeStunCameraShaker;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];

        private Rigidbody _rb;
        private bool _isInitialized;
        private float _destroyTimeLeft;

        #region Unity Functions

        private void Start() => Init();

        private void Update()
        {
            _destroyTimeLeft -= Time.deltaTime;
            if (_destroyTimeLeft <= 0)
            {
                ProjectileDestroy();
            }
        }

        private void FixedUpdate() => _rb.AddForce(Vector3.down * _additionalGravity, ForceMode.Force);

        #endregion Unity Functions

        #region Core Projectile Functions

        public void LaunchProjectile(Vector3 direction)
        {
            Init();
            _rb.velocity = _launchVelocity * direction;
            _destroyTimeLeft = _projectileDestroyTime;
        }

        public void ProjectileDestroy()
        {
            var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _effectRadius, _hitColliders, _stunMask);
            if (_debugIsActive)
            {
                DebugExtension.DebugWireSphere(transform.position, Color.red, _effectRadius, _debugDisplayDuration);
            }
            for (var i = 0; i < targetsHit; i++)
            {
                if (_hitColliders[i].TryGetComponent(out PlayerController targetController))
                {
                    var targetTransform = targetController.transform;
                    var position = targetTransform.position;
                    var stunObject = Instantiate(_stunPrefab, position, Quaternion.identity, targetTransform);
                    var stun = stunObject.GetComponent<Ability_StunDurationBased>();
                    targetController.AddExternalAbility(stun);
                }
            }

            var damageTargetsHit = Physics.OverlapSphereNonAlloc(transform.position, _effectRadius, _hitColliders, _damageMask);
            if (_debugIsActive)
            {
                DebugExtension.DebugWireSphere(transform.position, Color.blue, _effectRadius, _debugDisplayDuration);
            }
            for (var i = 0; i < damageTargetsHit; i++)
            {
                if (_hitColliders[i].TryGetComponent(out HealthAndDamage healthAndDamage))
                {
                    healthAndDamage.TakeDamage(_damageAmount);
                }
            }

            if (_hasSecondaryGrenades)
            {
                var angleDifference = 360.0f / _secondaryGrenadeCount;
                float startAngle = 0;

                for (var i = 0; i < _secondaryGrenadeCount; i++)
                {
                    var secondaryProjectile = Instantiate(_secondaryGrenadePrefab, transform.position, Quaternion.Euler(0, startAngle, 0));
                    var stunGrenade = secondaryProjectile.GetComponent<StunGrenade>();
                    var ownerData = secondaryProjectile.GetComponent<OwnerData>();

                    var forward = secondaryProjectile.transform.forward;

                    ownerData.OwnerId = _ownerData.OwnerId;
                    stunGrenade.LaunchProjectile(forward);
                    startAngle += angleDifference;
                }
            }

            var spawnPosition = transform.position;
            Instantiate(_destroyEffect, spawnPosition, Quaternion.identity);
            CustomCameraController.Instance.StartShake(_grenadeStunCameraShaker, spawnPosition);
            Destroy(gameObject);
        }

        public void ProjectileHit(Collider other)
        {
        }

        #endregion Core Projectile Functions

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