#region

using System.Collections.Generic;
using System.Linq;
using CustomCamera;
using HealthSystem;
using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;
using Utils.Misc;
using World;

#endregion

namespace Player.Type_3
{
    public class Type_3_Primary_DarkShoot : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _damageEffectPrefab;
        [SerializeField] private GameObject _raycastPointsPrefab;

        [Header("Components")]
        [SerializeField] private Animator _playerAnimator;

        [Header("Shoot Data")]
        [SerializeField] private int _animMinIndex;
        [SerializeField] private int _animMaxIndex;
        [SerializeField] private float _fireRate;
        [SerializeField] private LayerMask _attackMask;
        [SerializeField] private int _damageAmount;
        [SerializeField] private float _raycastDistance;

        [Header("Overheat")]
        [SerializeField] private float _overheatTime;
        [SerializeField] private float _overheatCooldownMultiplier;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _cameraShaker;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private Transform _playerCinemachine;
        private GameObject _raycastParent;
        private List<Transform> _raycastPoints;

        private HealthAndDamage _validHitTarget;
        private Vector3 _hitPoint;

        private float _nextShootTime;
        private float _currentOverheatTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) =>
            base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0 && _validHitTarget != null;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (Time.time >= _nextShootTime)
            {
                _nextShootTime = Time.time + _fireRate;
                _currentOverheatTime += _fireRate;

                if (_currentOverheatTime >= _overheatTime)
                {
                    _currentCooldownDuration = _cooldownDuration;
                    _currentOverheatTime = 0;
                    _abilityEnd = true;
                }

                _validHitTarget.TakeDamage(_damageAmount);
                Instantiate(_damageEffectPrefab, _hitPoint, Quaternion.identity);

                _playerAnimator.SetInteger(PlayerStaticData.Type_3_Primary, Random.Range(_animMinIndex, _animMaxIndex + 1));
                CustomCameraController.Instance.StartShake(_cameraShaker);
                HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
            }

            var inputKey = playerController.GetKeyForAbilityTrigger(_abilityTrigger);
            if (inputKey.KeyReleasedThisFrame || !inputKey.KeyPressed)
            {
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            base.UnityStartDelegate(playerController);

            _playerCinemachine = GameObject.FindGameObjectWithTag(TagManager.PlayerCinemachineController).transform;
            _raycastParent = Instantiate(_raycastPointsPrefab, _playerCinemachine.position, Quaternion.identity, _playerCinemachine);
            _raycastPoints = _raycastParent.GetComponentsInChildren<Transform>().ToList();
        }

        public override void UnityFixedUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityFixedUpdateDelegate(playerController);
            if (_currentOverheatTime > 0 && _abilityEnd)
            {
                _currentOverheatTime -= WorldTimeManager.Instance.FixedUpdateTime * _overheatCooldownMultiplier;
            }
        }

        public override void UnityUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityUpdateDelegate(playerController);
            UpdateValidHitTarget();
        }

        private void OnDestroy() => Destroy(_raycastParent);

        #endregion Unity Functions

        #region Utils

        private void UpdateValidHitTarget()
        {
            _validHitTarget = null;
            _hitPoint = Vector3.zero;

            foreach (var raycastPoint in _raycastPoints)
            {
                var hit = Physics.Raycast(raycastPoint.position, _playerCinemachine.forward, out var hitInfo, _raycastDistance, _attackMask);
                if (_debugIsActive)
                {
                    Debug.DrawRay(raycastPoint.position, _playerCinemachine.forward * _raycastDistance, Color.red, _debugDisplayDuration);
                }

                if (hit)
                {
                    if (hitInfo.transform.TryGetComponent(out HealthAndDamage healthAndDamage))
                    {
                        _validHitTarget = healthAndDamage;
                        _hitPoint = hitInfo.point;
                        break;
                    }
                }
            }

            if (_currentCooldownDuration <= 0)
            {
                HUD_PlayerAbilityDisplay.Instance.UpdateOverlay(_abilityTrigger, _validHitTarget == null ? 1 : 0);
            }
        }

        #endregion Utils
    }
}