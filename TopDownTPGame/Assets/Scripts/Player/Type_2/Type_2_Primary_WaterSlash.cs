#region

using System;
using System.Collections.Generic;
using Ability_Scripts.Projectiles;
using HealthSystem;
using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;
using Utils.Structs;
using Random = UnityEngine.Random;

#endregion

namespace Player.Type_2
{
    // This attack is comprised of 1 Right Slash, 1 Left Slash and then a Shoot front attack
    public partial class Type_2_Primary_WaterSlash : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _shootFrontPrefab;

        [Header("Components")]
        [SerializeField] private GameObject _parent;
        [SerializeField] private GameObject _rightHandSword;
        [SerializeField] private PlayerBaseShootController _shootController;
        [SerializeField] private Animator _playerAnimator;

        [Header("Anim Data")]
        [SerializeField] private int _animMinIndex;
        [SerializeField] private int _animMaxIndex;
        [SerializeField] private List<SwordPositionRotation> _swordPositionRotations;

        [Header("Slash Data")]
        [SerializeField] private float _resetDuration;
        [SerializeField] private float _slashDuration;

        [Header("General Data")]
        [SerializeField] private float _overheatTime;
        [SerializeField] private float _overheatAmountPerShot;
        [SerializeField] private float _overheatCooldownMultiplier;

        private WaterControlState _waterControlState;
        private bool _abilityEnd;

        private float _currentOverheatTime;

        private float _currentTime;
        private float _lastTriggeredTime;

        #region Ability Functions

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            switch (_waterControlState)
            {
                case WaterControlState.LeftSlash:
                case WaterControlState.RightSlash:
                    UpdateSlashTimer();
                    break;

                case WaterControlState.ShootFront:
                    LaunchFrontProjectile();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            _abilityEnd = true;
            _playerAnimator.SetInteger(PlayerStaticData.Type_2_Primary, 0);
            _rightHandSword.SetActive(false);
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            var currentTime = Time.time;
            var difference = currentTime - _lastTriggeredTime;
            if (difference > _resetDuration)
            {
                SetState(WaterControlState.LeftSlash);
            }

            _abilityEnd = false;
            if (_currentCooldownDuration > 0)
            {
                SetState(WaterControlState.LeftSlash);
            }
            else
            {
                _currentOverheatTime += _overheatAmountPerShot;
                if (_currentOverheatTime >= _overheatTime)
                {
                    _currentCooldownDuration = _cooldownDuration;
                    _currentOverheatTime = 0;
                }
            }

            if (_waterControlState != WaterControlState.ShootFront)
            {
                TriggerSwordAttackAnim();
            }
            else
            {
                _playerAnimator.SetTrigger(PlayerStaticData.Type_2_PrimaryFront);
            }

            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
        }

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            base.UnityStartDelegate(playerController);
            SetState(WaterControlState.LeftSlash);
        }

        public override void UnityFixedUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityFixedUpdateDelegate(playerController);
            if (_currentOverheatTime > 0 && _abilityEnd)
            {
                _currentOverheatTime -= Time.fixedDeltaTime * _overheatCooldownMultiplier;
            }
        }

        #endregion Unity Functions

        #region Ability Updates

        private void TriggerSwordAttackAnim()
        {
            var animIndex = Random.Range(_animMinIndex, _animMaxIndex + 1);
            _playerAnimator.SetInteger(PlayerStaticData.Type_2_Primary, animIndex);

            _rightHandSword.SetActive(true);
            _rightHandSword.transform.localPosition = _swordPositionRotations[animIndex - 1].Position;
            _rightHandSword.transform.localRotation = Quaternion.Euler(_swordPositionRotations[animIndex - 1].Rotation);

            _currentTime = 0;
        }

        private void UpdateSlashTimer()
        {
            _currentTime += Time.fixedDeltaTime;
            if (_currentTime >= _slashDuration)
            {
                IncrementCurrentState();
                _lastTriggeredTime = Time.time;
                _abilityEnd = true;
            }
        }

        private GameObject CreateFrontProjectile()
        {
            var spawnPosition = _shootController.GetShootPosition();
            var projectile = Instantiate(_shootFrontPrefab, spawnPosition, Quaternion.identity);
            return projectile;
        }

        private void LaunchFrontProjectile()
        {
            var frontSlashObject = CreateFrontProjectile();
            var direction = _shootController.GetShootLookDirection();
            var simpleProj = frontSlashObject.GetComponent<SimpleProjectile>();
            var simpleDamageTrigger = frontSlashObject.GetComponent<SimpleDamageTrigger>();

            simpleProj.LaunchProjectile(direction);
            simpleDamageTrigger.SetParent(_parent);

            IncrementCurrentState();
            _lastTriggeredTime = Time.time;
            _abilityEnd = true;
        }

        #endregion Ability Updates

        #region State Control

        private void SetState(WaterControlState waterControlState) => _waterControlState = waterControlState;

        private void IncrementCurrentState()
        {
            switch (_waterControlState)
            {
                case WaterControlState.LeftSlash:
                    SetState(WaterControlState.RightSlash);
                    break;

                case WaterControlState.RightSlash:
                    SetState(WaterControlState.ShootFront);
                    break;

                case WaterControlState.ShootFront:
                    SetState(WaterControlState.LeftSlash);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion State Control

        #region Enums

        private enum WaterControlState
        {
            LeftSlash,
            RightSlash,
            ShootFront,
        };

        #endregion Enums
    }
}