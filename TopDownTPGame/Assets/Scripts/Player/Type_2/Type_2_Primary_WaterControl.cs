using Player.Base;
using Player.Common;
using Projectiles;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

namespace Player.Type_2
{
    // This attack is comprised of 1 Right Slash, 1 Left Slash and then a Shoot front attack
    public class Type_2_Primary_WaterControl : Ability
    {
        [Header("Components")]
        [SerializeField] private GameObject _slashSidePrefab;
        [SerializeField] private GameObject _slashFrontPrefab;
        [SerializeField] private Transform _shootPoint;

        [Header("Water Lines Data")]
        [SerializeField] private SplineContainer _leftSlash;
        [SerializeField] private SplineContainer _rightSlash;
        [SerializeField] private float _slashDuration;
        [SerializeField] private float _resetDuration;

        private WaterControlState _waterControlState;
        private bool _abilityEnd;

        private GameObject _sideSlashObject;
        private float _currentTime;
        private float _lastTriggeredTime;

        #region Unity Functions

        private void Start() => SetState(WaterControlState.LeftSlash);

        #endregion Unity Functions

        public override bool AbilityCanStart() => true;

        public override bool AbilityNeedsToEnd() => _abilityEnd;

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
            }
        }

        public override void EndAbility() => _abilityEnd = true;

        public override void StartAbility()
        {
            float currentTime = Time.time;
            float difference = currentTime - _lastTriggeredTime;
            if (difference > _resetDuration)
            {
                SetState(WaterControlState.LeftSlash);
            }

            _abilityEnd = false;
            switch (_waterControlState)
            {
                case WaterControlState.LeftSlash:
                case WaterControlState.RightSlash:
                    _sideSlashObject = CreateSlashPrefab(_waterControlState);
                    _currentTime = 0;
                    break;

                case WaterControlState.ShootFront:
                    // Don't do anything here...
                    break;
            }
        }

        #region Ability Updates

        private GameObject CreateSlashPrefab(WaterControlState waterControlState)
        {
            Vector3 spawnPosition = Vector3.zero;
            switch (waterControlState)
            {
                case WaterControlState.LeftSlash:
                    _leftSlash.EvaluatePosition(0, 0);
                    break;

                case WaterControlState.RightSlash:
                    _rightSlash.EvaluatePosition(0, 0);
                    break;

                case WaterControlState.ShootFront:
                    throw new System.Exception("Invalid State for this GameObject");
            }

            GameObject projectile = Instantiate(_slashSidePrefab, spawnPosition, Quaternion.identity);
            return projectile;
        }

        private GameObject CreateFrontSlash()
        {
            Vector3 spawnPosition = _shootPoint.position;

            GameObject projectile = Instantiate(_slashFrontPrefab, spawnPosition, Quaternion.identity);
            return projectile;
        }

        private void UpdateSlash(WaterControlState waterControlState)
        {
            Assert.IsNotNull(_sideSlashObject);

            float percent = _currentTime / _slashDuration;
            Vector3 position = Vector3.zero;

            switch (waterControlState)
            {
                case WaterControlState.LeftSlash:
                    position = _leftSlash.EvaluatePosition(0, percent);
                    break;

                case WaterControlState.RightSlash:
                    position = _rightSlash.EvaluatePosition(0, percent);
                    break;

                case WaterControlState.ShootFront:
                    throw new System.Exception("Invalid State for this GameObject");
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
            GameObject frontSlashObject = CreateFrontSlash();
            Vector3 direction = transform.forward;
            SimpleOneShotForwardProjectile simpleProj = frontSlashObject.GetComponent<SimpleOneShotForwardProjectile>();
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