#region

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
        [SerializeField] private GameObject _dashEffectPrefab;

        [Header("Components")]
        [SerializeField] private Animator _playerAnimator;

        [Header("Dash Data")]
        [SerializeField] private float _dashDuration;
        [SerializeField] private float _dashVelocity;

        [Header("Dash Effects Data")]
        [SerializeField] private Vector3 _dashEffectOffset;
        [SerializeField] private Vector3 _leftDashRotation;
        [SerializeField] private Vector3 _rightDashRotation;
        [SerializeField] private Vector3 _frontDashRotation;
        [SerializeField] private Vector3 _backDashRotation;

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
            _currentDashTimeLeft -= Time.fixedDeltaTime;

            // Basically when there is no input use forward only...
            if (ExtensionFunctions.IsNearlyEqual(_startCoreInput.x, 0) && ExtensionFunctions.IsNearlyEqual(_startCoreInput.y, 0))
            {
                _startCoreInput.y = 1;
            }

            var currentVelocity = playerController.CharacterVelocity;
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
            _playerAnimator.SetBool(PlayerStaticData.Type_1_Tertiary, false);
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

            var dashEffectRotation = coreInput.y switch
            {
                > 0 => _frontDashRotation,
                < 0 => _backDashRotation,
                _ => coreInput.x > 0 ? _rightDashRotation : _leftDashRotation
            };

            var characterTransform = transform;
            _dashEffectObject = Instantiate(_dashEffectPrefab, characterTransform.position, Quaternion.identity, characterTransform);
            _dashEffectObject.transform.localPosition += _dashEffectOffset;
            _dashEffectObject.transform.localRotation = Quaternion.Euler(dashEffectRotation);

            _playerAnimator.SetBool(PlayerStaticData.Type_1_Tertiary, true);
            _playerAnimator.SetFloat(PlayerStaticData.Type_1_TertiaryHorizontal, _startCoreInput.x);
            _playerAnimator.SetFloat(PlayerStaticData.Type_1_TertiaryVertical, _startCoreInput.y);

            _currentCooldownDuration = _cooldownDuration;
        }

        #endregion Ability Functions

        #region Specific Data

        public override Vector3 GetMovementData() => _computedVelocity;

        #endregion Specific Data
    }
}