using System;
using HealthSystem;
using UnityEngine;
using Utils.Misc;

namespace Ability_Scripts.Spawns
{
    public class Type_4_TurretController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Transform _turretTop;

        [Header("Turret Targeting Data")]
        [SerializeField] private float _targetingRadius;
        [SerializeField] private LayerMask _targetingMask;
        [SerializeField] private float _stayOnTargetMinDuration;
        [SerializeField] private float _targetLowestHealthDelay;

        [Header("Shooting Data")]
        [SerializeField] private float _warmUpTime;
        [SerializeField] private float _damagePerTick;

        private Collider[] _hitColliders = new Collider[StaticData.MaxCollidersCheck];

        private TurretState _turretState;
        private float _currentTimer;

        private int _ownerId;

        #region Unity Functions

        private void Update()
        {
            switch (_turretState)
            {
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
                if (_hitColliders[i].TryGetComponent(out HealthAndDamage healthAndDamage))
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
            }
        }

        private void UpdateTargetingState()
        {
        }

        #endregion Turret State Updates

        private bool HasDirectLineOfSight(Transform target)
        {
            var startPosition = _shootPoint.position;
            var direction = target.position - startPosition;
            var hit = Physics.Raycast(startPosition, direction, out var hitInfo, _targetingRadius, _targetingMask);
            return hit && hitInfo.transform.gameObject.GetInstanceID() == target.GetInstanceID();
        }

        private HealthAndDamage GetNearestHealthAndDamage()
        {
            var hitCount = Physics.OverlapSphereNonAlloc(_shootPoint.position, _targetingRadius, _hitColliders, _targetingMask);
            var closestDistance = float.MaxValue;
            HealthAndDamage closestHealthAndDamage = null;

            for (var i = 0; i < hitCount; i++)
            {
                if (_hitColliders[i].TryGetComponent(out HealthAndDamage healthAndDamage))
                {
                    if (!HasDirectLineOfSight(_hitColliders[i].transform))
                    {
                        continue;
                    }

                    var distance = Vector3.Distance(_hitColliders[i].transform.position, _shootPoint.position);
                    if (distance < closestDistance)
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
                if (_hitColliders[i].TryGetComponent(out HealthAndDamage healthAndDamage))
                {
                    if (!HasDirectLineOfSight(_hitColliders[i].transform))
                    {
                        continue;
                    }

                    var health = healthAndDamage.CurrentHealth;
                    if (health < lowestDamage)
                    {
                        lowestDamage = health;
                        lowestHealthAndDamage = healthAndDamage;
                    }
                }
            }

            return lowestHealthAndDamage;
        }

        private void SetTurretState(TurretState turretState) => _turretState = turretState;

        #endregion Utils

        #region Enums

        private enum TurretState
        {
            Idle,
            WarmUp,
            Targeting,
        }

        #endregion Enums
    }
}