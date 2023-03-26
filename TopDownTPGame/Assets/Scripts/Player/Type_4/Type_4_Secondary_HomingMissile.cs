#region

using Ability_Scripts.Projectiles;
using CustomCamera;
using HealthSystem;
using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;

#endregion

namespace Player.Type_4
{
    public class Type_4_Secondary_HomingMissile : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _homingMissilePrefab;

        [Header("Components")]
        [SerializeField] private GameObject _parent;
        [SerializeField] private PlayerBaseShootController _shootController;
        [SerializeField] private Type_4_DroneController _droneController;

        [Header("Spawn Data")]
        [SerializeField] private float _windUpTime;
        [SerializeField] private float _targetDistance;
        [SerializeField] private LayerMask _targetMask;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _cameraShaker;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private Transform _validTarget;
        private float _currentWindUpTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) =>
            base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0 && _validTarget != null;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentWindUpTime -= Time.fixedDeltaTime;
            if (_currentWindUpTime <= 0)
            {
                var shootPosition = _shootController.GetShootPosition();
                var direction = _shootController.GetShootLookDirection();

                var lookDirection = Quaternion.LookRotation(direction);
                var missile = Instantiate(_homingMissilePrefab, shootPosition, lookDirection);
                var homingTarget = missile.GetComponent<PlasmaHomingMissile>();
                var simpleDamageTrigger = missile.GetComponent<SimpleDamageTrigger>();

                homingTarget.SetTarget(_validTarget);
                simpleDamageTrigger.SetParent(_parent);

                _abilityEnd = true;
                _currentCooldownDuration = _cooldownDuration;

                _droneController.KnockbackDrone(PlayerStaticData.Type_4_SecondaryDroneKnockbackMultiplier);
                CustomCameraController.Instance.StartShake(_cameraShaker);
                HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController)
        {
            _abilityEnd = false;
            _currentWindUpTime = _windUpTime;
        }

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            base.UnityStartDelegate(playerController);
            _abilityEnd = true;
        }

        public override void UnityUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityUpdateDelegate(playerController);
            UpdateValidTarget();
        }

        #endregion Unity Functions

        #region Utils

        private void UpdateValidTarget()
        {
            // Don't change target when the ability is already activated...
            if (!_abilityEnd)
            {
                return;
            }

            _validTarget = null;

            var shootPosition = _shootController.GetShootPosition();
            var direction = _shootController.GetShootLookDirection();

            var hit = Physics.Raycast(shootPosition, direction, out var hitInfo, _targetDistance, _targetMask);
            if (_debugIsActive)
            {
                Debug.DrawRay(shootPosition, direction * _targetDistance, Color.red, _debugDisplayDuration);
            }

            if (hit)
            {
                _validTarget = hitInfo.transform;
            }

            if (_currentCooldownDuration <= 0)
            {
                HUD_PlayerAbilityDisplay.Instance.UpdateOverlay(_abilityTrigger, _validTarget == null ? 1 : 0);
            }
        }

        #endregion Utils
    }
}