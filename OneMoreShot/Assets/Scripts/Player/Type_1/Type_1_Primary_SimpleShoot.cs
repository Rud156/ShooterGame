﻿using CustomCamera;
using Player.Abilities;
using Player.Core;
using Projectiles;
using UI.Player;
using UnityEngine;
using Utils.Common;
using World;

namespace Player.Type_1
{
    public class Type_1_Primary_SimpleShoot : AbilityBase
    {
        private static readonly int Type_1_PrimaryAnimParam = Animator.StringToHash("Type_1_Primary");

        [Header("Prefabs")]
        [SerializeField] private GameObject _abilityProjectile;

        [Header("Shoot Data")]
        [SerializeField] private float _fireRate;
        [SerializeField] private float _overheatDurationAmount;
        [SerializeField] private float _overheatCooldownMultiplier;
        [SerializeField] private int _attackAnimCount;
        [SerializeField] private float _attackMovementFreezeDuration;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _abilityCameraShaker;

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
            if (_nextShootDuration <= 0)
            {
                _nextShootDuration = _fireRate;
                _currentOverheatTime += _fireRate;
                _playerController.ForcePlayerLookToMousePosition(_attackMovementFreezeDuration);

                var spawnPosition = _playerShootController.GetShootPosition();
                var direction = _playerShootController.GetShootLookDirection();

                var projectile = Instantiate(_abilityProjectile, spawnPosition, Quaternion.identity);
                var simpleProjectile = projectile.GetComponent<SimpleProjectile>();
                var ownerData = projectile.GetComponent<OwnerData>();

                ownerData.OwnerId = _ownerData.OwnerId;
                simpleProjectile.LaunchProjectile(direction);

                _playerAnimator.SetInteger(Type_1_PrimaryAnimParam, Random.Range(1, _attackAnimCount + 1));
                HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
                CustomCameraController.Instance.StartShake(_abilityCameraShaker);
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

            var inputKey = _playerController.GetKeyForAbilityTrigger(_abilityTrigger);
            if (inputKey.KeyReleasedThisFrame || !inputKey.KeyPressed)
            {
                _abilityMarkedForEnd = true;
            }
        }

        public override void AbilityEnd(PlayerController playerController) => _abilityMarkedForEnd = true;

        #endregion Core Ability Functions

        #region Ability Conditions

        public override bool AbilityNeedsToEnd(PlayerController playerController) => _abilityMarkedForEnd;

        #endregion Ability Conditions

        #region Unity Function Delegates

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

        #endregion Unity Function Delegates
    }
}