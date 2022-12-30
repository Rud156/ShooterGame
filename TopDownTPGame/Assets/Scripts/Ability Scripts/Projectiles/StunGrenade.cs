using UnityEngine;

namespace AbilityScripts.Projectiles
{
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

        private bool _isInitialized;
        private Rigidbody _rb;

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
            if (!_isSecondary)
            {
                float angleDifference = 360 / _secondaryGrenadeCount;
                float startAngle = 0;

                for (int i = 0; i < _secondaryGrenadeCount; i++)
                {
                    GameObject secondaryProjectile = Instantiate(_miniGrenadePrefab, transform.position, Quaternion.Euler(0, startAngle, 0));
                    Vector3 forward = secondaryProjectile.transform.forward;

                    StunGrenade stunGrenade = secondaryProjectile.GetComponent<StunGrenade>();
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