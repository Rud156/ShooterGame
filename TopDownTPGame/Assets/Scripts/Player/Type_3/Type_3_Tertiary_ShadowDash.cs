#region

using System;
using Player.Base;
using Player.Common;
using Player.UI;
using UnityEngine;
using Utils.Misc;

#endregion

namespace Player.Type_3
{
    public class Type_3_Tertiary_ShadowDash : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private DashEffect _dashEffectForward;
        [SerializeField] private DashEffect _dashEffectBackward;
        [SerializeField] private DashEffect _dashEffectLeft;
        [SerializeField] private DashEffect _dashEffectRight;

        [Header("Components")]
        [SerializeField] private Transform _cameraHolder;

        [Header("Dash Charges")]
        [SerializeField] private int _dashCharges;
        [SerializeField] private float _dashDuration;
        [SerializeField] private float _dashVelocity;

        [Header("Dash Float")]
        [SerializeField] private float _dashEndFloatDuration;
        [SerializeField] private float _dashEndFloatFallMultiplier;

        private GameObject _dashEffectObject;

        private int _currentDashesLeftCount;
        private float _currentDashTimeLeft;
        private Vector3 _computedVelocity;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => _currentDashesLeftCount > 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _currentDashTimeLeft <= 0;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var coreInput = playerController.GetCoreMoveInput();
            _currentDashTimeLeft -= Time.fixedDeltaTime;

            // Basically when there is no input use forward only...
            if (ExtensionFunctions.IsNearlyEqual(coreInput.x, 0) && ExtensionFunctions.IsNearlyEqual(coreInput.y, 0))
            {
                coreInput.y = 1;
            }

            var forward = _cameraHolder.forward;
            var right = _cameraHolder.right;

            _computedVelocity = forward * coreInput.y + right * coreInput.x;
            _computedVelocity = _dashVelocity * _computedVelocity.normalized;
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            playerController.PlayerConstantSpeedFallTimed(_dashEndFloatDuration, _dashEndFloatFallMultiplier);

            _currentDashesLeftCount -= 1;
            if (_currentDashesLeftCount <= 0)
            {
                _currentCooldownDuration = _cooldownDuration;
            }

            Destroy(_dashEffectObject);
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            _currentDashTimeLeft = _dashDuration;
            DisplayDashEffect(playerController);
        }

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            OnAbilityCooldownComplete += HandleCooldownComplete;

            base.UnityStartDelegate(playerController);

            _currentDashesLeftCount = _dashCharges;
        }

        private void OnDestroy() => OnAbilityCooldownComplete -= HandleCooldownComplete;

        public override void UnityUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityUpdateDelegate(playerController);
            UpdateDashCountChanged();
        }

        #endregion Unity Functions

        #region Specific Data

        public override Vector3 GetMovementData() => _computedVelocity;

        #endregion Specific Data

        #region Utils

        private void HandleCooldownComplete()
        {
            _currentDashesLeftCount = Mathf.Clamp(_currentDashesLeftCount + 1, 0, _dashCharges);
            if (_currentDashesLeftCount < _dashCharges)
            {
                _currentCooldownDuration = _cooldownDuration;
            }
        }

        private void UpdateDashCountChanged() => PlayerAbilityDisplay.Instance.UpdateStackCount(AbilityTrigger.Tertiary, _currentDashesLeftCount);

        private void DisplayDashEffect(BasePlayerController playerController)
        {
            var coreInput = playerController.GetCoreMoveInput();
            // Basically when there is no input use forward only...
            if (ExtensionFunctions.IsNearlyEqual(coreInput.x, 0) && ExtensionFunctions.IsNearlyEqual(coreInput.y, 0))
            {
                coreInput.y = 1;
            }

            GameObject dashEffectPrefab;
            Vector3 dashEffectOffset;
            Vector3 dashEffectRotation;

            switch (coreInput.y)
            {
                case > 0:
                    dashEffectPrefab = _dashEffectForward.effectPrefab;
                    dashEffectOffset = _dashEffectForward.effectOffset;
                    dashEffectRotation = _dashEffectForward.effectLocalRotation;
                    break;

                case < 0:
                    dashEffectPrefab = _dashEffectBackward.effectPrefab;
                    dashEffectOffset = _dashEffectBackward.effectOffset;
                    dashEffectRotation = _dashEffectBackward.effectLocalRotation;
                    break;

                default:
                {
                    if (coreInput.x > 0)
                    {
                        dashEffectPrefab = _dashEffectLeft.effectPrefab;
                        dashEffectOffset = _dashEffectLeft.effectOffset;
                        dashEffectRotation = _dashEffectLeft.effectLocalRotation;
                    }
                    else
                    {
                        dashEffectPrefab = _dashEffectRight.effectPrefab;
                        dashEffectOffset = _dashEffectRight.effectOffset;
                        dashEffectRotation = _dashEffectRight.effectLocalRotation;
                    }

                    break;
                }
            }

            _dashEffectObject = Instantiate(dashEffectPrefab, _cameraHolder.position, Quaternion.identity, _cameraHolder);
            _dashEffectObject.transform.localPosition += dashEffectOffset;
            _dashEffectObject.transform.localRotation = Quaternion.Euler(dashEffectRotation);
        }

        #endregion Utils

        #region Structs

        [Serializable]
        private struct DashEffect
        {
            public GameObject effectPrefab;
            public Vector3 effectOffset;
            public Vector3 effectLocalRotation;
        }

        #endregion Structs
    }
}