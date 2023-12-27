using Player.Abilities;
using Player.Core;
using UI.Player;
using UnityEngine;
using UnityEngine.Assertions;
using Utils.Misc;

namespace Player.Type_3
{
    public class Type_3_Secondary_ShadowDash : AbilityBase
    {
        private static readonly int Type_3_SecondaryAnimParam = Animator.StringToHash("Type_3_Secondary");

        [Header("Prefabs")]
        [SerializeField] private GameObject _dashEffectPrefab;

        [Header("Dash Effects Data")]
        [SerializeField] private Vector3 _frontDashRotation;

        [Header("Dash Data")]
        [SerializeField] private int _maxDashChargeCount;
        [SerializeField] private float _dashDuration;
        [SerializeField] private float _dashVelocity;

        private GameObject _dashEffectInstance;

        private int _currentDashesAvailableCount;
        private float _currentDashDurationLeft;
        private Vector2 _startingCoreInput;
        private Vector3 _dashComputedVelocity;

        #region Core Ability Functions

        public override void AbilityStart(PlayerController playerController)
        {
            base.AbilityStart(playerController);
            Assert.IsNull(_dashEffectInstance, "Dash has to be NULL here...");

            var coreInput = _playerController.CoreMovementInput;
            if (ExtensionFunctions.IsNearlyZero(coreInput.x) && ExtensionFunctions.IsNearlyZero(coreInput.y))
            {
                coreInput.y = 1;
            }

            _startingCoreInput = coreInput;

            var characterTransform = transform;
            _dashEffectInstance = Instantiate(_dashEffectPrefab, characterTransform.position, Quaternion.identity, characterTransform);
            _dashEffectInstance.transform.localRotation = Quaternion.Euler(_frontDashRotation);

            _currentCooldownDuration = _abilityCooldownDuration;
            _currentDashDurationLeft = _dashDuration;
            _playerAnimator.SetBool(Type_3_SecondaryAnimParam, true);
            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
        }

        public override void AbilityFixedUpdate(PlayerController playerController, float fixedDeltaTime)
        {
            _currentDashDurationLeft -= fixedDeltaTime;

            var characterTransform = playerController.transform;
            var forward = characterTransform.forward;
            var right = characterTransform.right;

            // Override X and Z
            _dashComputedVelocity = forward * _startingCoreInput.y + right * _startingCoreInput.x;
            _dashComputedVelocity = _dashVelocity * _dashComputedVelocity.normalized;
            _dashComputedVelocity.y = 0;
        }

        public override void AbilityUpdate(PlayerController playerController, float deltaTime)
        {
        }

        public override void AbilityEnd(PlayerController playerController)
        {
            Assert.IsNotNull(_dashEffectInstance, "Dash effect cannot be NULL here..");
            _playerAnimator.SetBool(Type_3_SecondaryAnimParam, false);
            Destroy(_dashEffectInstance);

            _currentDashesAvailableCount -= 1;
            if (_currentDashesAvailableCount <= 0)
            {
                _currentCooldownDuration = _abilityCooldownDuration;
            }
        }

        #endregion Core Ability Functions

        #region Ability Conditions

        public override bool AbilityCanStart(PlayerController playerController) => base.AbilityCanStart(playerController) && _currentDashesAvailableCount > 0;

        public override bool AbilityNeedsToEnd(PlayerController playerController) => _currentDashDurationLeft <= 0;

        #endregion Ability Conditions

        #region Unity Function Delegates

        public override void UnityStartDelegate(PlayerController playerController)
        {
            OnAbilityCooldownComplete += HandleCooldownComplete;
            base.UnityStartDelegate(playerController);
            _currentDashesAvailableCount = _maxDashChargeCount;
        }

        public override void UnityUpdateDelegate(PlayerController playerController)
        {
            base.UnityUpdateDelegate(playerController);
            UpdateDashCountChanged();
        }

        public override void UnityDestroyDelegate(PlayerController playerController) => OnAbilityCooldownComplete -= HandleCooldownComplete;

        #endregion Unity Function Delegates

        #region Getters

        public override Vector3 GetMovementData() => _dashComputedVelocity;

        #endregion Getters

        #region Misc

        private void HandleCooldownComplete()
        {
            _currentDashesAvailableCount = Mathf.Clamp(_currentDashesAvailableCount + 1, 0, _maxDashChargeCount);
            if (_currentDashesAvailableCount < _maxDashChargeCount)
            {
                _currentCooldownDuration = _abilityCooldownDuration;
            }
        }

        private void UpdateDashCountChanged() => HUD_PlayerAbilityDisplay.Instance.UpdateCounter(_abilityTrigger, $"{_currentDashesAvailableCount}", true);

        #endregion Misc
    }
}