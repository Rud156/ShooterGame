#region

using System;
using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using Player.UI;
using UnityEngine;
using Utils.Misc;

#endregion

namespace Player.Type_4
{
    public class Type_4_Tertiary_Satchel : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _satchelPrefab;

        [Header("Components")]
        [SerializeField] private AbilityPrefabInitializer _prefabInit;
        [SerializeField] private Transform _cameraHolder;
        [SerializeField] private BaseShootController _shootController;

        [Header("Satchel Data")]
        [SerializeField] private float _minSatchelAffectRadius;
        [SerializeField] private float _satchelAffectRadius;
        [SerializeField] private float _satchelVelocity;
        [SerializeField] private float _airControlMultiplier;
        [SerializeField] private float _minForceDuration;
        [SerializeField] private float _maxForceDuration;
        [SerializeField] private float _satchelGravityMultiplier;

        [Header("Dash Charges")]
        [SerializeField] private int _satchelCount;

        [Header("Post Start Filled")]
        [SerializeField] private Transform _orbitShootPoint;
        [SerializeField] private Transform _staticShootPoint;

        private SatchelNade _satchelObject;
        private bool _abilityEnd;

        private Vector3 _computedVelocity;
        private Vector3 _direction;
        private float _satchelSpawnedDuration;

        private int _currentSatchelsLeft;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (_satchelSpawnedDuration <= 0)
            {
                InitialSatchelActivation(playerController);
            }
            else
            {
                UpdateSatchelMovement(playerController);
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController)
        {
            _abilityEnd = false;
            _satchelSpawnedDuration = 0;
            _computedVelocity = Vector3.zero;
        }

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            OnAbilityCooldownComplete += HandleCooldownComplete;
            base.UnityStartDelegate(playerController);

            _prefabInit.AbilityPrefabInit();
            _orbitShootPoint = transform.Find("CameraHolder/Type_4_CameraHolderPrefab(Clone)/BelowShootPoint");
            _staticShootPoint = transform.Find("Type_4_NormalPrefab(Clone)/BelowShootPoint");

            _currentSatchelsLeft = _satchelCount;
        }

        private void OnDestroy()
        {
            OnAbilityCooldownComplete -= HandleCooldownComplete;
        }

        public override void UnityUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityUpdateDelegate(playerController);
            UpdateDashCountChanged();
        }

        #endregion Unity Functions

        #region Specific Data

        public override Vector3 GetMovementData() => _computedVelocity;

        #endregion Specific Data

        #region Ability Updates

        private void InitialSatchelActivation(BasePlayerController playerController)
        {
            if (_satchelObject == null)
            {
                var direction = _shootController.GetShootLookDirection();

                var shootPosition = playerController.IsGrounded ? _staticShootPoint.position : _orbitShootPoint.position;
                var satchel = Instantiate(_satchelPrefab, shootPosition, Quaternion.identity);
                var satchelNade = satchel.GetComponent<SatchelNade>();
                satchelNade.LaunchProjectile(direction);

                _satchelObject = satchelNade;
                _abilityEnd = true;

                _currentSatchelsLeft -= 1;
                if (_currentSatchelsLeft <= 0)
                {
                    _currentCooldownDuration = _cooldownDuration;
                }
            }
            else
            {
                var distance = Vector3.Distance(transform.position, _satchelObject.transform.position);
                if (distance > _satchelAffectRadius)
                {
                    _abilityEnd = true;
                }
                else
                {
                    var direction = transform.position - _satchelObject.transform.position;
                    var mappedDuration = _maxForceDuration;
                    if (distance > _minSatchelAffectRadius)
                    {
                        mappedDuration = ExtensionFunctions.Map(distance, _minSatchelAffectRadius, _satchelAffectRadius, _maxForceDuration, _minForceDuration);
                    }

                    _satchelSpawnedDuration = mappedDuration;
                    _direction = direction.normalized;
                    _computedVelocity = Vector3.zero;
                }

                _satchelObject.ProjectileDestroy();
                _satchelObject = null;
            }
        }

        private void UpdateSatchelMovement(BasePlayerController playerController)
        {
            _satchelSpawnedDuration -= Time.fixedDeltaTime;

            var coreInput = playerController.GetCoreMoveInput();
            var forward = _cameraHolder.forward;
            var right = _cameraHolder.right;

            var satchelMovement = forward * coreInput.y + right * coreInput.x;
            satchelMovement.y = 0;
            satchelMovement = _airControlMultiplier * _satchelVelocity * satchelMovement.normalized;

            _computedVelocity = _direction * _satchelVelocity;
            _computedVelocity.x += satchelMovement.x;
            _computedVelocity.z += satchelMovement.z;
            _computedVelocity.y += Physics.gravity.y * _satchelGravityMultiplier;

            if (_satchelSpawnedDuration < 0)
            {
                _abilityEnd = true;
            }
        }

        private void HandleCooldownComplete()
        {
            _currentSatchelsLeft = Mathf.Clamp(_currentSatchelsLeft + 1, 0, _satchelCount);
            if (_currentSatchelsLeft < _satchelCount)
            {
                _currentCooldownDuration = _cooldownDuration;
            }
        }

        private void UpdateDashCountChanged() => PlayerAbilityDisplay.Instance.UpdateStackCount(AbilityTrigger.Tertiary, _currentSatchelsLeft);

        #endregion Ability Updates
    }
}