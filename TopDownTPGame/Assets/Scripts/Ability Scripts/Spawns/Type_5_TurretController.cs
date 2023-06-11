#region

using System;
using HealthSystem;
using Player.Common;
using UnityEngine;
using Utils.Common;
using Random = UnityEngine.Random;

#endregion

namespace Ability_Scripts.Spawns
{
    public class Type_5_TurretController : MonoBehaviour
    {
        private static readonly int AlphaClip = Shader.PropertyToID("_AlphaClip");
        private const float OneSecond = 1;

        [Header("Components")]
        [SerializeField] private OwnerData _ownerIdData;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Collider _turretCollider;
        [SerializeField] private Transform _turretTop;
        [SerializeField] private GameObject _turretRingParticle;

        [Header("Laser")]
        [SerializeField] private GameObject _laserLineGameObject;
        [SerializeField] private GameObject _laserStartEnd;
        [SerializeField] private GameObject _laserTargetEnd;
        [SerializeField] private LineRenderer _laserLine;

        [Header("Turret Meshes")]
        [SerializeField] private Renderer _turretTopMesh;
        [SerializeField] private Renderer _turretBottomMesh;

        [Header("Turret State Data")]
        [SerializeField] private float _activationAlphaChangeRate;
        [SerializeField] private float _destroyAlphaChangeRate;

        [Header("Turret Targeting Data")]
        [SerializeField] private float _targetingRadius;
        [SerializeField] private LayerMask _targetingMask;
        [SerializeField] private float _stayOnTargetMinDuration;
        [SerializeField] [Range(0, 1)] private float _targetLowestHealthRatio;

        [Header("Shooting Data")]
        [SerializeField] private float _windupTime;
        [SerializeField] private int _damagePerSec;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];

        [SerializeField] private TurretState _turretState;
        [SerializeField] private TurretTargetingState _turretTargetingState;

        private float _floatData1;
        private float _floatData2;

        private Material _turretTopMaterial;
        private Material _turretBottomMaterial;
        private Transform _currentTarget;
        private HealthAndDamage _targetHealthAndDamage;

        private Action _callbackFunc;

        #region Unity Functions

        private void Start()
        {
            SetTurretState(TurretState.InActive);
            SetTurretTargetingState(TurretTargetingState.None);
        }

        private void Update()
        {
            _turretCollider.enabled = false;

            switch (_turretState)
            {
                case TurretState.InActive:
                    break;

                case TurretState.Activating:
                    UpdateActivatingState();
                    break;

                case TurretState.Idle:
                    UpdateIdleState();
                    break;

                case TurretState.WindUp:
                    UpdateWindUpState();
                    break;

                case TurretState.Targeting:
                    UpdateTargetingState();
                    break;

                case TurretState.Destroy:
                    UpdateDestroyState();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            _turretCollider.enabled = true;
        }

        #endregion Unity Functions

        #region External Functions

        public void SetDamageCallback(Action callback) => _callbackFunc = callback;

        public void ActivateTurret()
        {
            _floatData1 = 1;
            _turretRingParticle.SetActive(true);
            _turretTopMaterial = _turretTopMesh.material;
            _turretBottomMaterial = _turretBottomMesh.material;

            SetTurretState(TurretState.Activating);
            SetTurretTargetingState(TurretTargetingState.None);
        }

        public void DestroyTurret()
        {
            _floatData1 = 0;
            SetTurretState(TurretState.Destroy);
            SetTurretTargetingState(TurretTargetingState.None);
        }

        #endregion External Functions

        #region Utils

        #region Turret State Updates

        private void UpdateActivatingState()
        {
            _floatData1 -= Time.deltaTime * _activationAlphaChangeRate;
            if (_floatData1 <= 0)
            {
                _floatData1 = 0;
                SetTurretState(TurretState.Idle);
            }

            _turretTopMaterial.SetFloat(AlphaClip, _floatData1);
            _turretBottomMaterial.SetFloat(AlphaClip, _floatData1);
        }

        private void UpdateIdleState()
        {
            var hitCount = Physics.OverlapSphereNonAlloc(_shootPoint.position, _targetingRadius, _hitColliders, _targetingMask);
            if (_debugIsActive)
            {
                DebugExtension.DebugWireSphere(_shootPoint.position, Color.red, _targetingRadius, _debugDisplayDuration);
            }

            var hasTargetInLos = false;
            for (var i = 0; i < hitCount; i++)
            {
                var hitOwnerData = _hitColliders[i].GetComponent<OwnerData>();
                if (hitOwnerData == null || hitOwnerData.OwnerId == _ownerIdData.OwnerId)
                {
                    continue;
                }

                if (_hitColliders[i].TryGetComponent(out HealthAndDamage _))
                {
                    var distance = Vector3.Distance(_hitColliders[i].transform.position, _shootPoint.position);
                    if (distance <= _targetingRadius && HasDirectLineOfSight(_hitColliders[i].transform))
                    {
                        hasTargetInLos = true;
                        break;
                    }
                }
            }

            if (hasTargetInLos)
            {
                _floatData1 = _windupTime;
                SetTurretState(TurretState.WindUp);
            }
        }

        private void UpdateWindUpState()
        {
            _floatData1 -= Time.deltaTime;
            if (_floatData1 <= 0)
            {
                SetTurretState(TurretState.Targeting);
                SetTurretTargetingState(TurretTargetingState.FindTarget);
            }
        }

        private void UpdateTargetingState()
        {
            switch (_turretTargetingState)
            {
                case TurretTargetingState.None:
                    break;

                case TurretTargetingState.FindTarget:
                    UpdateTurretFindTargetState();
                    break;

                case TurretTargetingState.TrackAndDamageTarget:
                    UpdateTurretTrackAndDamageState();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateDestroyState()
        {
            _floatData1 += Time.deltaTime * _destroyAlphaChangeRate;
            if (_floatData1 >= 1)
            {
                _floatData1 = 1;
                Destroy(gameObject);
            }

            _turretTopMaterial.SetFloat(AlphaClip, _floatData1);
            _turretBottomMaterial.SetFloat(AlphaClip, _floatData1);
        }

        #endregion Turret State Updates

        #region Turret Targetting State Updates

        private void UpdateTurretFindTargetState()
        {
            var random = Random.value;
            if (random <= _targetLowestHealthRatio)
            {
                _targetHealthAndDamage = GetLowestHealthAndDamage();
                if (_targetHealthAndDamage == null)
                {
                    SetTurretState(TurretState.Idle);
                    SetTurretTargetingState(TurretTargetingState.None);
                    return;
                }

                _currentTarget = _targetHealthAndDamage.transform;
            }
            else
            {
                _targetHealthAndDamage = GetNearestHealthAndDamage();
                if (_targetHealthAndDamage == null)
                {
                    SetTurretState(TurretState.Idle);
                    SetTurretTargetingState(TurretTargetingState.None);
                    return;
                }

                _currentTarget = _targetHealthAndDamage.transform;
            }

            _floatData1 = OneSecond;
            _floatData2 = _stayOnTargetMinDuration;
            SetTurretTargetingState(TurretTargetingState.TrackAndDamageTarget);
        }

        private void UpdateTurretTrackAndDamageState()
        {
            if (_currentTarget == null || _targetHealthAndDamage == null)
            {
                SetTurretState(TurretState.Idle);
                SetTurretTargetingState(TurretTargetingState.None);
                return;
            }

            _floatData2 -= Time.deltaTime;
            if (_floatData2 <= 0)
            {
                SetTurretTargetingState(TurretTargetingState.FindTarget);
            }

            _floatData1 -= Time.deltaTime;
            if (_floatData1 <= 0)
            {
                _targetHealthAndDamage.TakeDamage(_damagePerSec);
                _callbackFunc?.Invoke();
                _floatData1 = OneSecond;
            }

            var targetPosition = _currentTarget.transform.position;
            var shootPosition = _shootPoint.position;
            var direction = targetPosition - shootPosition;
            var lookRotation = Quaternion.LookRotation(direction);

            _turretTop.rotation = lookRotation;
            _laserTargetEnd.transform.position = targetPosition;
            _laserLine.SetPosition(0, shootPosition);
            _laserLine.SetPosition(1, targetPosition);

            var distance = Vector3.Distance(targetPosition, shootPosition);
            if (distance > _targetingRadius)
            {
                SetTurretState(TurretState.Idle);
                SetTurretTargetingState(TurretTargetingState.None);
            }
        }

        #endregion Turret Targetting State Updates

        #region Enemy Functions

        private bool HasDirectLineOfSight(Transform target)
        {
            var startPosition = _shootPoint.position;
            var direction = target.position - startPosition;
            var hit = Physics.Raycast(startPosition, direction, out var hitInfo, _targetingRadius, _targetingMask);
            return hit && hitInfo.transform.gameObject.GetComponent<OwnerData>().OwnerId == target.gameObject.GetComponent<OwnerData>().OwnerId;
        }

        private HealthAndDamage GetNearestHealthAndDamage()
        {
            var hitCount = Physics.OverlapSphereNonAlloc(_shootPoint.position, _targetingRadius, _hitColliders, _targetingMask);
            var closestDistance = float.MaxValue;
            HealthAndDamage closestHealthAndDamage = null;

            for (var i = 0; i < hitCount; i++)
            {
                var hitOwnerData = _hitColliders[i].GetComponent<OwnerData>();
                if (hitOwnerData == null || hitOwnerData.OwnerId == _ownerIdData.OwnerId)
                {
                    continue;
                }

                if (_hitColliders[i].TryGetComponent(out HealthAndDamage healthAndDamage))
                {
                    if (!HasDirectLineOfSight(_hitColliders[i].transform))
                    {
                        continue;
                    }

                    var distance = Vector3.Distance(_hitColliders[i].transform.position, _shootPoint.position);
                    if (distance < closestDistance && distance < _targetingRadius)
                    {
                        closestHealthAndDamage = healthAndDamage;
                        closestDistance = distance;
                    }
                }
            }

            return closestHealthAndDamage;
        }

        private HealthAndDamage GetLowestHealthAndDamage()
        {
            var hitCount = Physics.OverlapSphereNonAlloc(_shootPoint.position, _targetingRadius, _hitColliders, _targetingMask);
            var lowestDamage = float.MaxValue;
            HealthAndDamage lowestHealthAndDamage = null;

            for (var i = 0; i < hitCount; i++)
            {
                var hitOwnerData = _hitColliders[i].GetComponent<OwnerData>();
                if (hitOwnerData == null || hitOwnerData.OwnerId == _ownerIdData.OwnerId)
                {
                    continue;
                }

                if (_hitColliders[i].TryGetComponent(out HealthAndDamage healthAndDamage))
                {
                    if (!HasDirectLineOfSight(_hitColliders[i].transform))
                    {
                        continue;
                    }

                    var health = healthAndDamage.CurrentHealth;
                    var distance = Vector3.Distance(_hitColliders[i].transform.position, _shootPoint.position);
                    if (health < lowestDamage && distance < _targetingRadius)
                    {
                        lowestDamage = health;
                        lowestHealthAndDamage = healthAndDamage;
                    }
                }
            }

            return lowestHealthAndDamage;
        }

        #endregion Enemy Functions

        #region State Changes

        private void SetTurretState(TurretState turretState)
        {
            _turretState = turretState;
            if (_laserStartEnd != null) // Since this function is also called Indirectly when the Player gets destroyed it causes a Unity Error
            {
                _laserStartEnd.SetActive(turretState is TurretState.WindUp or TurretState.Targeting);
            }
        }

        private void SetTurretTargetingState(TurretTargetingState turretTargetingState)
        {
            _turretTargetingState = turretTargetingState;

            if (_laserLineGameObject != null && _laserTargetEnd != null)
            {
                if (turretTargetingState == TurretTargetingState.TrackAndDamageTarget)
                {
                    _laserLineGameObject.SetActive(true);
                    _laserTargetEnd.SetActive(true);
                }
                else
                {
                    _laserLineGameObject.SetActive(false);
                    _laserTargetEnd.SetActive(false);
                }
            }
        }

        #endregion State Changes

        #endregion Utils

        #region Enums

        private enum TurretState
        {
            InActive,
            Activating,
            Idle,
            WindUp,
            Targeting,
            Destroy,
        }

        private enum TurretTargetingState
        {
            None,
            FindTarget,
            TrackAndDamageTarget,
        }

        #endregion Enums
    }
}