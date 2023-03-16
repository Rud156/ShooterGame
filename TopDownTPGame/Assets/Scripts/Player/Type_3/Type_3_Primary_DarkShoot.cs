#region

using System.Collections.Generic;
using System.Linq;
using HealthSystem;
using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;
using Utils.Misc;
using Random = UnityEngine.Random;

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

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private Transform _playerCinemachine;
        private GameObject _raycastParent;
        private List<Transform> _raycastPoints;

        private float _nextShootTime;
        private float _currentOverheatTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0;

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
                            healthAndDamage.TakeDamage(_damageAmount);
                            Instantiate(_damageEffectPrefab, hitInfo.point, Quaternion.identity);
                            break;
                        }
                    }
                }

                _playerAnimator.SetInteger(PlayerStaticData.Type_3_Primary, Random.Range(_animMinIndex, _animMaxIndex + 1));
                HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlash(_abilityTrigger);
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
                _currentOverheatTime -= Time.fixedDeltaTime * _overheatCooldownMultiplier;
            }
        }

        private void OnDestroy() => Destroy(_raycastParent);

        #endregion Unity Functions
    }
}