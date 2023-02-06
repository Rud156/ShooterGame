#region

using Player.Base;
using Player.Type_4;
using UnityEngine;
using Utils.Misc;

#endregion

namespace Ability_Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class SatchelNade : MonoBehaviour, IProjectile
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _satchLaunchPrefab;
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
        [SerializeField] private float _maxRadiusForAffect;
        [SerializeField] private float _minRadiusForMaxAffect;
        [SerializeField] private float _minVelocity;
        [SerializeField] private float _maxVelocity;
        [SerializeField] private LayerMask _affectMask;

        private Rigidbody _rb;
        private bool _isInitialized;

        private Vector3 _currentVelocity;

        private bool _isStuck;
        private float _destroyTimeLeft;

        #region Unity Functions

        private void Start() => Init();

        private void Update()
        {
            if (!_isStuck)
            {
                CheckCollisionWithObject();
            }
        }

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
            LaunchPlayersWithSatchel();
            Instantiate(_satchelExplodePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

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
            _isStuck = false;
        }

        private void LaunchPlayersWithSatchel()
        {
            var colliders = Physics.OverlapSphere(transform.position, _maxRadiusForAffect, _affectMask);
            DebugExtension.DebugWireSphere(transform.position, Color.white, _maxRadiusForAffect, 10);
            foreach (var targetCollider in colliders)
            {
                if (targetCollider.TryGetComponent(out BasePlayerController targetController))
                {
                    var hitObjectPosition = targetCollider.transform.position;
                    var position = transform.position;

                    var direction = (hitObjectPosition - position).normalized;
                    var distance = Vector3.Distance(position, hitObjectPosition);
                    var isInLos = Physics.Raycast(position, direction, out var hitInfo, distance, _affectMask);

                    if (isInLos && hitInfo.collider.gameObject.GetInstanceID() == targetCollider.gameObject.GetInstanceID())
                    {
                        var velocityApplied = distance <= _minRadiusForMaxAffect
                            ? _maxVelocity
                            : ExtensionFunctions.Map(distance, _minRadiusForMaxAffect, _maxRadiusForAffect, _maxVelocity, _minVelocity);

                        Debug.Log($"Velocity: {velocityApplied}");

                        var satchelMovementObject = Instantiate(_satchLaunchPrefab, hitObjectPosition, Quaternion.identity, targetCollider.transform);
                        var satchelMovement = satchelMovementObject.GetComponent<Type_4_Tertiary_SatchelMovement>();
                        satchelMovement.ApplySatchelMovement(direction, velocityApplied);
                        targetController.CheckAndAddExternalAbility(satchelMovement);
                    }
                }
            }
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