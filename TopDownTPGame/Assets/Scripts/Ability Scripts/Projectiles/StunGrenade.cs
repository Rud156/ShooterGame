#region

using System;
using CustomCamera;
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
        [SerializeField] private GameObject _stunPrefab;
        [SerializeField] private GameObject _destroyEffect;

        [Header("General Grenade Data")]
        [SerializeField] private float _additionalGravity;
        [SerializeField] private float _launchVelocity;
        [SerializeField] private float _projectileDestroyTime;
        [SerializeField] private int _damageAmount;

        [Header("Secondary Grenades")]
        [SerializeField] private bool _hasSecondaryGrenades;
        [SerializeField] private int _secondaryGrenadeCount;
        [SerializeField] private GameObject _secondaryGrenadePrefab;

        [Header("Stun Data")]
        [SerializeField] private float _stunEffectRadius;
        [SerializeField] private LayerMask _stunMask;

        [Header("Camera Data")]
        [SerializeField] private CameraShakerInRange _cameraShaker;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];

        private Rigidbody _rb;
        private bool _isInitialized;
        private float _destroyTimeLeft;

        private Type_5_Secondary_StunGrenade _parentSpawner;
        private Action<Collider, Type_5_Secondary_StunGrenade.StunGrenadeType> _callbackFunc;

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

        #region External Functions

        public void LaunchProjectile(Vector3 direction)
        {
            Init();
            _rb.velocity = direction * _launchVelocity;
            _destroyTimeLeft = _projectileDestroyTime;
        }

        public void SetCollisionCallback(Action<Collider, Type_5_Secondary_StunGrenade.StunGrenadeType> callback) => _callbackFunc = callback;

        public void SetParentSpawner(Type_5_Secondary_StunGrenade parentSpawner) => _parentSpawner = parentSpawner;

        public void ProjectileDestroy()
        {
            var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _stunEffectRadius, _hitColliders, _stunMask);
            if (_debugIsActive)
            {
                DebugExtension.DebugWireSphere(transform.position, Color.red, _stunEffectRadius, _debugDisplayDuration);
            }

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
                    _callbackFunc?.Invoke(_hitColliders[i], Type_5_Secondary_StunGrenade.StunGrenadeType.Primary);
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
                    var forward = secondaryProjectile.transform.forward;

                    if (_parentSpawner != null)
                    {
                        _parentSpawner.AddCallbackFunctionToSecondaryGrenade(stunGrenade);
                    }

                    stunGrenade.LaunchProjectile(forward);
                    startAngle += angleDifference;
                }
            }

            Instantiate(_destroyEffect, transform.position, Quaternion.identity);
            CustomCameraController.Instance.StartShake(_cameraShaker, transform.position);
            Destroy(gameObject);
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
        }

        #endregion Utils
    }
}