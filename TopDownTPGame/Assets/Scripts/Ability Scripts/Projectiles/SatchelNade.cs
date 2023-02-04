#region

using UnityEngine;

#endregion

namespace Ability_Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class SatchelNade : MonoBehaviour, IProjectile
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _satchelExplodePrefab;

        [Header("Satchel Data")]
        [SerializeField] private float _additionalGravity;
        [SerializeField] private float _satchelLaunchVelocity;
        [SerializeField] private float _velocityDecreaseRate;
        [SerializeField] private float _destroyDuration;

        [Header("Satchel Stop Data")]
        [SerializeField] private float _raycastDistance;
        [SerializeField] private LayerMask _raycastMask;

        [Header("Satchel Explode Launch Data")]
        [SerializeField] private float _affectRadius;
        [SerializeField] private float _minRaidusForMaxAffect;
        [SerializeField] private float _minAffectAcceleration;
        [SerializeField] private float _maxAffectAcceleration;

        private Rigidbody _rb;
        private BoxCollider _collider;
        private bool _isInitialized;

        private Vector3 _currentVelocity;

        private bool _isStuck;
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

            if (!_isStuck)
            {
                UpdateSatchelVelocity();
                CheckCollisionWithObject();
            }
        }

        #endregion Unity Functions

        #region Externals Functions

        public void LaunchProjectile(Vector3 direction)
        {
            Init();
            _currentVelocity = direction * _satchelLaunchVelocity;
            _rb.velocity = _currentVelocity;
        }

        public void ProjectileDestroy()
        {
            Instantiate(_satchelExplodePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        public void ProjectileHit(Collider other)
        {
        }

        public void LaunchPlayersWithSatchel()
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
            _collider = GetComponent<BoxCollider>();

            _isInitialized = true;
            _destroyTimeLeft = _destroyDuration;
            _isStuck = false;
        }

        private void UpdateSatchelVelocity()
        {
            var velocity = _currentVelocity;

            var yVelocity = velocity.y;
            yVelocity += Physics.gravity.y * _additionalGravity;

            var xVelocity = velocity.x;
            if (xVelocity < 0)
            {
                xVelocity += _velocityDecreaseRate * Time.fixedDeltaTime;
            }
            else
            {
                xVelocity -= _velocityDecreaseRate * Time.fixedDeltaTime;
            }

            var zVelocity = velocity.z;
            if (zVelocity < 0)
            {
                zVelocity += _velocityDecreaseRate * Time.fixedDeltaTime;
            }
            else
            {
                zVelocity -= _velocityDecreaseRate * Time.fixedDeltaTime;
            }

            _currentVelocity.x = xVelocity;
            _currentVelocity.y = yVelocity;
            _currentVelocity.z = zVelocity;

            _rb.velocity = _currentVelocity;
        }

        private void CheckCollisionWithObject()
        {
            for (var i = 0; i < 6; i++)
            {
                var direction = GetDirectionFromIndex(i);
                var hit = Physics.Raycast(transform.position, direction, out var hitInfo, _raycastDistance, _raycastMask);
                Debug.DrawRay(transform.position, direction * _raycastDistance, Color.white, 10);
                if (hit)
                {
                    var normal = hitInfo.normal;
                    var rotation = Quaternion.LookRotation(normal);
                    transform.rotation = rotation;
                    transform.position = hitInfo.point;
                    _isStuck = true;

                    DisableSatchelMovement();
                    break;
                }
            }
        }

        private void DisableSatchelMovement()
        {
            _rb.velocity = Vector3.zero;
            _rb.isKinematic = true;
            _collider.enabled = false;
        }

        private Vector3 GetDirectionFromIndex(int index)
        {
            // 0: Left, 1: Right, 2: Forward, 3:Backward, 4: Up, 5: Down
            var satchelTransform = transform;
            var forward = satchelTransform.forward;
            var right = satchelTransform.right;
            var up = satchelTransform.up;

            return index switch
            {
                0 => -right,
                1 => right,
                2 => forward,
                3 => -forward,
                4 => up,
                5 => -up,
                _ => Vector3.zero
            };
        }

        #endregion Utils
    }
}