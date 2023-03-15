#region

using System.Collections.Generic;
using System.Linq;
using HealthSystem;
using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;
using Utils.Misc;

#endregion

namespace Player.Type_3
{
    public class Type_3_Primary_DarkShoot : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _damageEffectPrefab;

        [Header("Components")]
        [SerializeField] private AbilityPrefabInitializer _abilityPrefabInitializer;

        [Header("Shoot Data")]
        [SerializeField] private float _fireRate;
        [SerializeField] private LayerMask _attackMask;
        [SerializeField] private int _damageAmount;
        [SerializeField] private float _raycastDistance;

        [Header("Overheat")]
        [SerializeField] private float _overheatTime;
        [SerializeField] private float _overheatCooldownMultiplier;

        [Header("Post Start Filled")]
        [SerializeField] private List<Transform> _raycastPoints;

        private Transform _playerCinemachine;

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

            _abilityPrefabInitializer.AbilityPrefabInit();
            _playerCinemachine = GameObject.FindGameObjectWithTag(TagManager.PlayerCinemachineController).transform;
            var raycastParent = _playerCinemachine.Find("Type_3_CameraPrefab(Clone)/RaycastPoints");
            _raycastPoints = raycastParent.GetComponentsInChildren<Transform>().ToList();
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
    }
}