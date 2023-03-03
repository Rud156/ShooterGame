#region

using System;
using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;
using UnityEngine.Assertions;
using Utils.Misc;

#endregion

namespace Player.Type_1
{
    public class Type_1_Tertiary_Dash : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private DashEffect _dashEffectForward;
        [SerializeField] private DashEffect _dashEffectBackward;
        [SerializeField] private DashEffect _dashEffectLeft;
        [SerializeField] private DashEffect _dashEffectRight;

        [Header("Components")]
        [SerializeField] private Animator _playerAnimator;

        [Header("Dash Data")]
        [SerializeField] private float _dashDuration;
        [SerializeField] private float _dashVelocity;

        private float _currentDashTimeLeft;
        private Vector3 _computedVelocity;
        private Vector2 _startCoreInput;

        private GameObject _dashEffectObject;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) =>
            base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0 && _currentDashTimeLeft <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _currentDashTimeLeft <= 0;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var currentVelocity = playerController.CharacterVelocity;

            _currentDashTimeLeft -= Time.fixedDeltaTime;

            // Basically when there is no input use forward only...
            if (ExtensionFunctions.IsNearlyEqual(_startCoreInput.x, 0) && ExtensionFunctions.IsNearlyEqual(_startCoreInput.y, 0))
            {
                _startCoreInput.y = 1;
            }

            var characterTransform = transform;
            var forward = characterTransform.forward;
            var right = characterTransform.right;

            // Override X and Z
            _computedVelocity = forward * _startCoreInput.y + right * _startCoreInput.x;
            _computedVelocity = _dashVelocity * _computedVelocity.normalized;
            _computedVelocity.y = currentVelocity.y;
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            Assert.IsNotNull(_dashEffectObject, "Dash effect cannot be NULL here..");
            _playerAnimator.SetBool(StaticData.Type_1_Tertiary, false);
            Destroy(_dashEffectObject);
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            Assert.IsNull(_dashEffectObject, "Dash has to be NULL here...");

            _currentDashTimeLeft = _dashDuration;
            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlash(_abilityTrigger);

            var coreInput = playerController.GetCoreMoveInput();
            _startCoreInput = coreInput;

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

            var characterTransform = transform;
            _dashEffectObject = Instantiate(dashEffectPrefab, characterTransform.position, Quaternion.identity, characterTransform);
            _dashEffectObject.transform.localPosition += dashEffectOffset;
            _dashEffectObject.transform.localRotation = Quaternion.Euler(dashEffectRotation);

            _playerAnimator.SetBool(StaticData.Type_1_Tertiary, true);
            _playerAnimator.SetFloat(StaticData.Type_1_TertiaryHorizontal, _startCoreInput.x);
            _playerAnimator.SetFloat(StaticData.Type_1_TertiaryVertical, _startCoreInput.y);

            _currentCooldownDuration = _cooldownDuration;
        }

        #endregion Ability Functions

        #region Specific Data

        public override Vector3 GetMovementData() => _computedVelocity;

        #endregion Specific Data

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