using CustomCamera;
using HeallthSystem;
using Player.Abilities;
using Player.Core;
using System.Collections.Generic;
using System.Linq;
using UI.Player;
using UnityEngine;
using World;

namespace Player.Type_3
{
    public class Type_3_Primary_DarkShoot : AbilityBase
    {
        private static readonly int Type_3_PrimaryAnimParam = Animator.StringToHash("Type_3_Primary");

        [Header("Prefabs")]
        [SerializeField] private GameObject _damageEffectPrefab;
        [SerializeField] private GameObject _raycastPointsPrefab;

        [Header("Damage Data")]
        [SerializeField] private int _damageAmount;

        [Header("Shoot Data")]
        [SerializeField] private int _attackAnimCount;
        [SerializeField] private float _attackMovementFreezeDuration;
        [SerializeField] private float _fireRate;
        [SerializeField] private LayerMask _raycastMask;
        [SerializeField] private float _raycastDistance;

        [Header("Overheat")]
        [SerializeField] private float _overheatDurationAmount;
        [SerializeField] private float _overheatCooldownMultiplier;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _abilityCameraShaker;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private List<Transform> _raycastPoints;
        private Vector3 _hitPosition;
        private HealthAndDamage _validTargetHealthDamage;
        private bool _targetFound;

        private float _nextShootDuration;
        private bool _abilityMarkedForEnd;

        private float _currentOverheatTime;

        #region Core Ability Functions

        public override void AbilityStart(PlayerController playerController)
        {
            base.AbilityStart(playerController);
            _abilityMarkedForEnd = false;
        }

        public override void AbilityFixedUpdate(PlayerController playerController, float fixedDeltaTime)
        {
            if (_abilityMarkedForEnd)
            {
                return;
            }

            if (_nextShootDuration <= 0)
            {
                _playerController.ForcePlayerLookToMousePosition(_attackMovementFreezeDuration);
                UpdateValidHitTarget();
                if (_targetFound)
                {
                    _nextShootDuration = _fireRate;
                    _currentOverheatTime += _fireRate;

                    Instantiate(_damageEffectPrefab, _hitPosition, Quaternion.identity);
                    _validTargetHealthDamage.TakeDamage(_damageAmount);

                    _playerAnimator.SetInteger(Type_3_PrimaryAnimParam, Random.Range(1, _attackAnimCount + 1));
                    HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
                    CustomCameraController.Instance.StartShake(_abilityCameraShaker);
                }
            }
            else
            {
                _nextShootDuration -= fixedDeltaTime;
            }

            if (_currentOverheatTime >= _overheatDurationAmount)
            {
                _currentCooldownDuration = _abilityCooldownDuration;
                _currentOverheatTime = 0;
                _abilityMarkedForEnd = true;
            }
        }

        public override void AbilityUpdate(PlayerController playerController, float deltaTime)
        {
            var inputKey = _playerController.GetKeyForAbilityTrigger(_abilityTrigger);
            if (inputKey.KeyReleasedThisFrame || !inputKey.KeyPressed)
            {
                _abilityMarkedForEnd = true;
            }
        }

        public override void AbilityEnd(PlayerController playerController) => _abilityMarkedForEnd = true;

        #endregion Core Ability Functions

        #region Ability Conditions

        public override bool AbilityCanStart(PlayerController playerController, bool ignoreCooldown = false) => base.AbilityCanStart(playerController);

        public override bool AbilityNeedsToEnd(PlayerController playerController) => _abilityMarkedForEnd;

        #endregion Ability Conditions

        #region Unity Function Delegates

        public override void UnityStartDelegate(PlayerController playerController)
        {
            base.UnityStartDelegate(playerController);

            var shootPoint = _playerShootController.GetShootPosition();
            var raycastParent = Instantiate(_raycastPointsPrefab, shootPoint, Quaternion.identity, transform);
            _raycastPoints = raycastParent.GetComponentsInChildren<Transform>().ToList();
        }

        public override void UnityFixedUpdateDelegate(PlayerController playerController, float fixedDeltaTime)
        {
            base.UnityFixedUpdateDelegate(playerController, fixedDeltaTime);
            if (_currentOverheatTime > 0)
            {
                _currentOverheatTime -= WorldTimeManager.Instance.FixedUpdateTime * _overheatCooldownMultiplier;
            }

            if (_abilityMarkedForEnd && _nextShootDuration > 0)
            {
                _nextShootDuration -= fixedDeltaTime;
            }
        }

        public override void UnityUpdateDelegate(PlayerController playerController)
        {
            base.UnityUpdateDelegate(playerController);
            UpdateValidHitTarget();
        }

        #endregion Unity Function Delegates

        #region Misc

        private void UpdateValidHitTarget()
        {
            _targetFound = false;
            _hitPosition = Vector3.zero;

            foreach (var raycastPoint in _raycastPoints)
            {
                var hit = Physics.Raycast(raycastPoint.position, transform.forward, out var hitInfo, _raycastDistance, _raycastMask);
                if (_debugIsActive)
                {
                    Debug.DrawRay(raycastPoint.position, transform.forward * _raycastDistance, Color.red, _debugDisplayDuration);
                }

                if (hit)
                {
                    if (hitInfo.transform.TryGetComponent<HealthAndDamage>(out HealthAndDamage healthAndDamage))
                    {
                        _validTargetHealthDamage = healthAndDamage;
                        _hitPosition = hitInfo.point;
                        _targetFound = true;
                        break;
                    }
                }
            }

            if (_currentCooldownDuration <= 0)
            {
                HUD_PlayerAbilityDisplay.Instance.UpdateOverlay(_abilityTrigger, _targetFound ? 0 : 1);
            }

            if (!_targetFound)
            {
                _validTargetHealthDamage = null;
                _hitPosition = Vector3.zero;
            }
        }

        #endregion Misc
    }
}