#region

using System;
using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
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
        [SerializeField] private GameObject _slashSidePrefab;
        [SerializeField] private GameObject _slashFrontPrefab;

        [Header("Components")]
        [SerializeField] private AbilityPrefabInitializer _prefabInit;

        [Header("Water Lines Data")]
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private AnimationCurve _leftEaseCurve;
        [SerializeField] private AnimationCurve _rightEaseCurve;
        [SerializeField] private float _slashDuration;
        [SerializeField] private float _resetDuration;

        [Header("Post Start Filled")]
        [SerializeField] private SplineContainer _leftSlash;
        [SerializeField] private SplineContainer _rightSlash;

        private WaterControlState _waterControlState;
        private bool _abilityEnd;

        private GameObject _sideSlashObject;
        private int _randomSlashIndex;
        private float _currentTime;
        private float _lastTriggeredTime;

        #region Unity Functions

        private void Start()
        {
            _prefabInit.AbilityPrefabInit();
            _leftSlash = transform.Find("Type_2_Prefab(Clone)/SlashPaths/LeftSlash").GetComponent<SplineContainer>();
            _rightSlash = transform.Find("Type_2_Prefab(Clone)/SlashPaths/RightSlash").GetComponent<SplineContainer>();

            SetState(WaterControlState.LeftSlash);
        }

        #endregion Unity Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

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
            var currentTime = Time.time;
            var difference = currentTime - _lastTriggeredTime;
            if (difference > _resetDuration)
            {
                SetState(WaterControlState.LeftSlash);
            }

            _abilityEnd = false;
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

        #region Ability Updates

        private GameObject CreateSlashPrefabAndUpdateRandomIndex(WaterControlState waterControlState)
        {
            var spawnPosition = Vector3.zero;
            switch (waterControlState)
            {
                case WaterControlState.LeftSlash:
                {
                    var totalSplines = _leftSlash.Splines.Count;
                    var randomIndex = Random.Range(0, totalSplines);
                    spawnPosition = _leftSlash.EvaluatePosition(randomIndex, 0);
                    _randomSlashIndex = randomIndex;
                }
                    break;

                case WaterControlState.RightSlash:
                {
                    var totalSplines = _leftSlash.Splines.Count;
                    var randomIndex = Random.Range(0, totalSplines);
                    spawnPosition = _rightSlash.EvaluatePosition(randomIndex, 0);
                    _randomSlashIndex = randomIndex;
                }
                    break;

                case WaterControlState.ShootFront:
                    throw new Exception("Invalid State for this GameObject");
            }

            var projectile = Instantiate(_slashSidePrefab, spawnPosition, Quaternion.identity);
            return projectile;
        }

        private GameObject CreateFrontSlash()
        {
            var spawnPosition = _shootPoint.position;

            var projectile = Instantiate(_slashFrontPrefab, spawnPosition, Quaternion.identity);
            return projectile;
        }

        private void UpdateSlash(WaterControlState waterControlState)
        {
            Assert.IsNotNull(_sideSlashObject);

            var percent = _currentTime / _slashDuration;
            var position = Vector3.zero;

            switch (waterControlState)
            {
                case WaterControlState.LeftSlash:
                {
                    var mappedPercent = _leftEaseCurve.Evaluate(percent);
                    position = _leftSlash.EvaluatePosition(_randomSlashIndex, mappedPercent);
                }
                    break;

                case WaterControlState.RightSlash:
                {
                    var mappedPercent = _rightEaseCurve.Evaluate(percent);
                    position = _rightSlash.EvaluatePosition(_randomSlashIndex, mappedPercent);
                }
                    break;

                case WaterControlState.ShootFront:
                    throw new Exception("Invalid State for this GameObject");
            }

            _sideSlashObject.transform.position = position;

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
            var frontSlashObject = CreateFrontSlash();
            var direction = transform.forward;
            var simpleProj = frontSlashObject.GetComponent<SimpleOneShotForwardProjectile>();
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