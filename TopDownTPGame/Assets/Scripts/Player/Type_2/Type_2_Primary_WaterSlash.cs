#region

using System;
using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

#endregion

namespace Player.Type_2
{
    // This attack is comprised of 1 Right Slash, 1 Left Slash and then a Shoot front attack
    public class Type_2_Primary_WaterSlash : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _slashLeftPrefab;
        [SerializeField] private GameObject _slashRightPrefab;
        [SerializeField] private GameObject _shootFrontPrefab;

        [Header("Components")]
        [SerializeField] private Transform parent;
        [SerializeField] private PlayerBaseShootController _shootController;

        [Header("Water Lines Data")]
        [SerializeField] private AnimationCurve _leftEaseCurve;
        [SerializeField] private AnimationCurve _rightEaseCurve;
        [SerializeField] private float _slashDuration;
        [SerializeField] private float _resetDuration;

        [Header("General Data")]
        [SerializeField] private float _overheatTime;
        [SerializeField] private float _overheatAmountPerShot;
        [SerializeField] private float _overheatCooldownMultiplier;

        [Header("Post Start Filled")]
        [SerializeField] private SplineContainer _leftSlash;
        [SerializeField] private SplineContainer _rightSlash;

        private WaterControlState _waterControlState;
        private bool _abilityEnd;

        private GameObject _sideSlashObject;
        private float _currentOverheatTime;

        private int _randomSlashIndex;
        private float _currentTime;
        private float _lastTriggeredTime;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController);

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            switch (_waterControlState)
            {
                case WaterControlState.LeftSlash:
                case WaterControlState.RightSlash:
                    UpdateSlash(_waterControlState);
                    break;

                case WaterControlState.ShootFront:
                    UpdateFrontSlash();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController)
        {
            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlash(_abilityTrigger);

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

            switch (_waterControlState)
            {
                case WaterControlState.LeftSlash:
                case WaterControlState.RightSlash:
                    _sideSlashObject = CreateSlashPrefabAndUpdateRandomIndex(_waterControlState);
                    _currentTime = 0;
                    break;

                case WaterControlState.ShootFront:
                    // Don't do anything here...
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            base.UnityStartDelegate(playerController);

            _leftSlash = parent.Find("Type_2_Prefab(Clone)/SlashPaths/LeftSlash").GetComponent<SplineContainer>();
            _rightSlash = parent.Find("Type_2_Prefab(Clone)/SlashPaths/RightSlash").GetComponent<SplineContainer>();

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

        private GameObject CreateSlashPrefabAndUpdateRandomIndex(WaterControlState waterControlState)
        {
            Vector3 spawnPosition;
            GameObject prefab;

            switch (waterControlState)
            {
                case WaterControlState.LeftSlash:
                {
                    var totalSplines = _leftSlash.Splines.Count;
                    var randomIndex = Random.Range(0, totalSplines);
                    spawnPosition = _leftSlash.EvaluatePosition(randomIndex, 0);
                    prefab = _slashLeftPrefab;
                    _randomSlashIndex = randomIndex;
                }
                    break;

                case WaterControlState.RightSlash:
                {
                    var totalSplines = _leftSlash.Splines.Count;
                    var randomIndex = Random.Range(0, totalSplines);
                    spawnPosition = _rightSlash.EvaluatePosition(randomIndex, 0);
                    prefab = _slashRightPrefab;
                    _randomSlashIndex = randomIndex;
                }
                    break;

                case WaterControlState.ShootFront:
                    throw new Exception("Invalid State for this GameObject");

                default:
                    throw new ArgumentOutOfRangeException(nameof(waterControlState), waterControlState, null);
            }

            var projectile = Instantiate(prefab, spawnPosition, Quaternion.identity);
            return projectile;
        }

        private GameObject CreateFrontBullet()
        {
            var spawnPosition = _shootController.GetShootPosition();
            var projectile = Instantiate(_shootFrontPrefab, spawnPosition, Quaternion.identity);
            return projectile;
        }

        private void UpdateSlash(WaterControlState waterControlState)
        {
            Assert.IsNotNull(_sideSlashObject);

            var percent = _currentTime / _slashDuration;
            Vector3 position;
            Vector3 rotation;

            switch (waterControlState)
            {
                case WaterControlState.LeftSlash:
                {
                    var mappedPercent = _leftEaseCurve.Evaluate(percent);
                    position = _leftSlash.EvaluatePosition(_randomSlashIndex, mappedPercent);
                    rotation = _leftSlash.EvaluateTangent(_randomSlashIndex, mappedPercent);
                }
                    break;

                case WaterControlState.RightSlash:
                {
                    var mappedPercent = _rightEaseCurve.Evaluate(percent);
                    position = _rightSlash.EvaluatePosition(_randomSlashIndex, mappedPercent);
                    rotation = _rightSlash.EvaluateTangent(_randomSlashIndex, mappedPercent);
                }
                    break;

                case WaterControlState.ShootFront:
                    throw new Exception("Invalid State for this GameObject");

                default:
                    throw new ArgumentOutOfRangeException(nameof(waterControlState), waterControlState, null);
            }

            _sideSlashObject.transform.position = position;
            _sideSlashObject.transform.rotation = Quaternion.LookRotation(rotation);

            _currentTime += Time.fixedDeltaTime;
            if (_currentTime >= _slashDuration)
            {
                Destroy(_sideSlashObject);
                IncrementCurrentState();

                _lastTriggeredTime = Time.time;
                _abilityEnd = true;
            }
        }

        private void UpdateFrontSlash()
        {
            var frontSlashObject = CreateFrontBullet();
            var direction = _shootController.GetShootLookDirection();
            var simpleProj = frontSlashObject.GetComponent<SimpleProjectile>();
            simpleProj.LaunchProjectile(direction);

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