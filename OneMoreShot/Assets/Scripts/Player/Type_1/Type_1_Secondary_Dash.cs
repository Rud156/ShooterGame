using Player.Abilities;
using Player.Core;
using UI.Player;
using UnityEngine;
using UnityEngine.Assertions;
using Utils.Misc;

namespace Player.Type_1
{
    public class Type_1_Secondary_Dash : AbilityBase
    {
        private static readonly int Type_1_SecondaryAnimParam = Animator.StringToHash("Type_1_Tertiary");

        [Header("Prefabs")]
        [SerializeField] private GameObject _dashEffectPrefab;

        [Header("Dash Effects Data")]
        [SerializeField] private Vector3 _dashEffectOffset;
        [SerializeField] private Vector3 _leftDashRotation;
        [SerializeField] private Vector3 _rightDashRotation;
        [SerializeField] private Vector3 _frontDashRotation;
        [SerializeField] private Vector3 _backDashRotation;

        [Header("Dash Data")]
        [SerializeField] private float _dashDuration;
        [SerializeField] private float _dashVelocity;

        private float _currentDashDurationLeft;
        private Vector3 _dashComputedVelocity;
        private Vector2 _startingCoreInput;

        private GameObject _dashEffectInstance;

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
            var dashEffectRotation = coreInput.y switch
            {
                > 0 => _frontDashRotation,
                < 0 => _backDashRotation,
                _ => coreInput.x > 0 ? _rightDashRotation : _leftDashRotation
            };

            var characterTransform = transform;
            _dashEffectInstance = Instantiate(_dashEffectPrefab, characterTransform.position, Quaternion.identity, characterTransform);
            _dashEffectInstance.transform.localPosition += _dashEffectOffset;
            _dashEffectInstance.transform.localRotation = Quaternion.Euler(dashEffectRotation);

            _currentCooldownDuration = _abilityCooldownDuration;
            _currentDashDurationLeft = _dashDuration;
            _playerAnimator.SetBool(Type_1_SecondaryAnimParam, true);
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
            _playerAnimator.SetBool(Type_1_SecondaryAnimParam, false);
            Destroy(_dashEffectInstance);
        }

        #endregion

        #region Ability Conditions

        public override bool AbilityCanStart(PlayerController playerController) =>
            base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0 && _currentDashDurationLeft <= 0;

        public override bool AbilityNeedsToEnd(PlayerController playerController) => _currentDashDurationLeft <= 0;

        #endregion

        #region Getters

        public override Vector3 MovementData() => _dashComputedVelocity;

        #endregion
    }
}