#region

using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_4
{
    public class Type_4_Secondary_HomingMissile : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _homingMissilePrefab;

        [Header("Components")]
        [SerializeField] private BaseShootController _shootController;

        [Header("Spawn Data")]
        [SerializeField] private float _windUpTime;
        [SerializeField] private float _targetDistance;
        [SerializeField] private LayerMask _targetMask;

        private float _currentWindUpTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentWindUpTime -= Time.fixedDeltaTime;
            if (_currentWindUpTime <= 0)
            {
                var shootPosition = _shootController.GetShootPosition();
                var direction = _shootController.GetShootLookDirection();

                var hit = Physics.Raycast(shootPosition, direction, out var hitInfo, _targetDistance, _targetMask);
                if (hit)
                {
                    var lookDirection = Quaternion.LookRotation(direction);
                    var missile = Instantiate(_homingMissilePrefab, shootPosition, lookDirection);
                    var homingTarget = missile.GetComponent<PlasmaHomingMissile>();

                    var target = hitInfo.collider.transform;
                    homingTarget.SetTarget(target);
                }

                _abilityEnd = true;
                _currentCooldownDuration = _cooldownDuration;
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController)
        {
            _abilityEnd = false;
            _currentWindUpTime = _windUpTime;
        }

        #endregion Ability Functions
    }
}