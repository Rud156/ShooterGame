#region

using System;
using HealthSystem;
using Player.Common;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

namespace Ability_Scripts.Spawns
{
    public class Type_5_TurretController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Transform _turretTop;
        [SerializeField] private GameObject _turretRingParticle;

        [Header("Laser")]
        [SerializeField] private GameObject _turretLaser;
        [SerializeField] private Transform _laserStartEnd;
        [SerializeField] private Transform _laserTargetEnd;
        [SerializeField] private LineRenderer _laserLine;

        [Header("Turret Targeting Data")]
        [SerializeField] private float _targetingRadius;
        [SerializeField] private LayerMask _targetingMask;
        [SerializeField] private float _stayOnTargetMinDuration;
        [SerializeField] [Range(0, 1)] private float _targetLowestHealthRatio;

        [Header("Shooting Data")]
        [SerializeField] private float _warmUpTime;
        [SerializeField] private int _damagePerSec;

        private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];

        private TurretState _turretState;
        private TurretTargetingState _turretTargetingState;

        private float _currentTargetingDuration;
        private float _currentTimer;

        private Transform _currentTarget;
        private HealthAndDamage _targetHealthAndDamage;

        private int _ownerId;

        #region Unity Functions

        private void Start()
        {
            SetTurretState(TurretState.InActive);
            SetTurretTargetingState(TurretTargetingState.FindTarget);
        }

        private void Update()
        {
            switch (_turretState)
            {
                case TurretState.InActive:
                    break;

                case TurretState.Idle:
                    UpdateIdleState();
                    break;

                case TurretState.WarmUp:
                    UpdateWarmUpState();
                    break;

                case TurretState.Targeting:
                    UpdateTargetingState();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void SetOwnerInstanceId(int ownerId) => _ownerId = ownerId;

        #endregion External Functions

        #region Utils

        #region Turret State Updates

        private void UpdateIdleState()
        {
            var hitCount = Physics.OverlapSphereNonAlloc(_shootPoint.position, _targetingRadius, _hitColliders, _targetingMask);
            var hasTargetInLos = false;

            for (var i = 0; i < hitCount; i++)
            {
                if (_hitColliders[i].TryGetComponent(out HealthAndDamage _))
                {
                    if (HasDirectLineOfSight(_hitColliders[i].transform))
                    {
                        hasTargetInLos = true;
                        break;
                    }
                }
            }

            if (hasTargetInLos)
            {
                _currentTimer = _warmUpTime;
                SetTurretState(TurretState.WarmUp);
            }
        }

        private void UpdateWarmUpState()
        {
            _currentTimer -= Time.fixedDeltaTime;
            if (_currentTimer <= 0)
            {
                SetTurretState(TurretState.Targeting);
                SetTurretTargetingState(TurretTargetingState.FindTarget);
            }
        }

        private void UpdateTargetingState()
        {
            switch (_turretTargetingState)
            {
                case TurretTargetingState.FindTarget:
                {
                    var random = Random.value;
                    if (random <= _targetLowestHealthRatio)
                    {
                        _targetHealthAndDamage = GetLowestHealthAndDamage();
                        if (_targetHealthAndDamage == null)
                        {
                            SetTurretState(TurretState.Idle);
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
                            return;
                        }

                        _currentTarget = _targetHealthAndDamage.transform;
                    }

                    _currentTimer = _stayOnTargetMinDuration;
                    _currentTargetingDuration = 1;
                    SetTurretTargetingState(TurretTargetingState.TrackAndDamageTarget);
                }
                    break;

                case TurretTargetingState.TrackAndDamageTarget:
                {
                    if (_currentTarget == null || _targetHealthAndDamage == null)
                    {
                        SetTurretState(TurretState.Idle);
                        return;
                    }

                    _currentTimer -= Time.fixedDeltaTime;
                    if (_currentTimer <= 0)
                    {
                        SetTurretTargetingState(TurretTargetingState.FindTarget);
                    }

                    _currentTargetingDuration -= Time.fixedDeltaTime;
                    if (_currentTargetingDuration <= 0)
                    {
                        _targetHealthAndDamage.TakeDamage(_damagePerSec);
                        _currentTargetingDuration = 1;
                    }

                    var targetPosition = _currentTarget.transform.position;
                    var shootPosition = _shootPoint.position;
                    var direction = targetPosition - shootPosition;
                    var lookRotation = Quaternion.LookRotation(direction);

                    _turretTop.rotation = lookRotation;
                    _laserStartEnd.position = shootPosition;
                    _laserTargetEnd.position = targetPosition;
                    _laserLine.SetPosition(0, shootPosition);
                    _laserLine.SetPosition(1, targetPosition);

                    var distance = Vector3.Distance(targetPosition, shootPosition);
                    if (distance > _targetingRadius)
                    {
                        SetTurretState(TurretState.Idle);
                    }
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion Turret State Updates

        #region Enemy Functions

        private bool HasDirectLineOfSight(Transform target)
        {
            var startPosition = _shootPoint.position;
            var direction = target.position - startPosition;
            var hit = Physics.Raycast(startPosition, direction, out var hitInfo, _targetingRadius, _targetingMask);
            return hit && hitInfo.transform.gameObject.GetInstanceID() == target.gameObject.GetInstanceID();
        }

        private HealthAndDamage GetNearestHealthAndDamage()
        {
            var hitCount = Physics.OverlapSphereNonAlloc(_shootPoint.position, _targetingRadius, _hitColliders, _targetingMask);
            var closestDistance = float.MaxValue;
            HealthAndDamage closestHealthAndDamage = null;

            for (var i = 0; i < hitCount; i++)
            {
                if (_hitColliders[i].gameObject.GetInstanceID() == _ownerId)
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
                if (_hitColliders[i].gameObject.GetInstanceID() == _ownerId)
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

        public void SetTurretActiveState(bool isActive)
        {
            SetTurretState(isActive ? TurretState.Idle : TurretState.InActive);
            _turretRingParticle.SetActive(isActive);
        }

        private void SetTurretState(TurretState turretState)
        {
            _turretState = turretState;
            if (turretState != TurretState.Targeting)
            {
                _turretLaser.SetActive(false);
            }
        }

        private void SetTurretTargetingState(TurretTargetingState turretTargetingState)
        {
            _turretTargetingState = turretTargetingState;
            _turretLaser.SetActive(turretTargetingState == TurretTargetingState.TrackAndDamageTarget);
        }

        #endregion State Changes

        #endregion Utils

        #region Enums

        private enum TurretState
        {
            InActive,
            Idle,
            WarmUp,
            Targeting,
        }

        private enum TurretTargetingState
        {
            FindTarget,
            TrackAndDamageTarget,
        }

        #endregion Enums
    }
}