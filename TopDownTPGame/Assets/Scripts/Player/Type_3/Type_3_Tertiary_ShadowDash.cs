#region

using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;
using Utils.Misc;
using World;

#endregion

namespace Player.Type_3
{
    public class Type_3_Tertiary_ShadowDash : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _dashEffectPrefab;

        [Header("Components")]
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private Transform _cinemachineFollow;

        [Header("Dash Charges")]
        [SerializeField] private int _dashCharges;
        [SerializeField] private float _dashDuration;
        [SerializeField] private float _dashVelocity;

        [Header("Dash Float")]
        [SerializeField] private float _dashEndFloatDuration;

        [Header("Dash Effects Data")]
        [SerializeField] private Vector3 _dashEffectOffset;
        [SerializeField] private Vector3 _leftDashRotation;
        [SerializeField] private Vector3 _rightDashRotation;
        [SerializeField] private Vector3 _frontDashRotation;
        [SerializeField] private Vector3 _backDashRotation;

        private GameObject _dashEffectObject;

        private int _currentDashesLeftCount;
        private float _currentDashTimeLeft;
        private Vector3 _computedVelocity;
        private Vector2 _startCoreInput;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController) && _currentDashesLeftCount > 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _currentDashTimeLeft <= 0;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentDashTimeLeft -= WorldTimeManager.Instance.FixedUpdateTime;

            var forward = _cinemachineFollow.forward;
            var right = _cinemachineFollow.right;

            _computedVelocity = forward * _startCoreInput.y + right * _startCoreInput.x;
            _computedVelocity = _dashVelocity * _computedVelocity.normalized;
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            playerController.PlayerConstantSpeedFallTimed(_dashEndFloatDuration);

            _currentDashesLeftCount -= 1;
            if (_currentDashesLeftCount <= 0)
            {
                _currentCooldownDuration = _cooldownDuration;
            }

            _playerAnimator.SetBool(PlayerStaticData.Type_3_Tertiary, false);
            Destroy(_dashEffectObject);
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            _currentDashTimeLeft = _dashDuration;

            var coreInput = playerController.GetCoreMoveInput();
            // Basically when there is no input use forward only...
            if (ExtensionFunctions.IsNearlyEqual(coreInput.x, 0) && ExtensionFunctions.IsNearlyEqual(coreInput.y, 0))
            {
                coreInput.y = 1;
            }

            _startCoreInput = coreInput;
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

            _playerAnimator.SetBool(PlayerStaticData.Type_3_Tertiary, true);
            _playerAnimator.SetFloat(PlayerStaticData.Type_3_TertiaryHorizontal, _startCoreInput.x);
            _playerAnimator.SetFloat(PlayerStaticData.Type_3_TertiaryVertical, _startCoreInput.y);

            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
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

        private void UpdateDashCountChanged() => HUD_PlayerAbilityDisplay.Instance.UpdateCounter(AbilityTrigger.Tertiary, $"{_currentDashesLeftCount}", true);

        #endregion Utils
    }
}