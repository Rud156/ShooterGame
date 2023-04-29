#region

using Player.Base;
using Player.Common;
using UI.DisplayManagers.Player;
using UnityEngine;
using World;

#endregion

namespace Player.Type_1
{
    public class Type_1_Ultimate_WarCryPulseAbility : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _ultimatePulsePrefab;

        [Header("Components")]
        [SerializeField] private Animator _playerAnimator;

        [Header("Ultimate Data")]
        [SerializeField] private float _windUptime;
        [SerializeField] private int _ultimateChargePerSec;
        [SerializeField] private int _maxUltimateChargeAmount;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;

        private GameObject _warCryObject;

        private float _currentWindUpTime;
        private bool _abilityEnd;

        // Ultimate Data
        private float _ultimateChargeTick;
        private int _currentUltimateAmount;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) =>
            base.AbilityCanStart(playerController) && _currentUltimateAmount >= _maxUltimateChargeAmount;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentWindUpTime -= WorldTimeManager.Instance.FixedUpdateTime;
            if (_currentWindUpTime <= 0)
            {
                var characterTransform = transform;
                _warCryObject = Instantiate(_ultimatePulsePrefab, characterTransform.position, Quaternion.identity, characterTransform);
                _warCryObject.GetComponent<Type_1_Ultimate_WarCryPulse>().SetTargetAnimatorData(_playerAnimator, PlayerStaticData.Type_1_Ultimate);
                _abilityEnd = true;

                HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController)
        {
            _currentWindUpTime = _windUptime;
            _currentUltimateAmount = 0;
            _abilityEnd = false;
        }

        public override void ClearAllAbilityData(BasePlayerController playerController)
        {
            if (_warCryObject != null)
            {
                Destroy(_warCryObject);
                _warCryObject = null;
            }
        }

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityUpdateDelegate(playerController);
            DisplayUltimateToHUD();
        }

        public override void UnityFixedUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityFixedUpdateDelegate(playerController);

            if (_currentUltimateAmount < _maxUltimateChargeAmount)
            {
                while (_ultimateChargeTick >= 1)
                {
                    _currentUltimateAmount += _ultimateChargePerSec;
                    _ultimateChargeTick -= 1;
                }

                _ultimateChargeTick += WorldTimeManager.Instance.FixedUpdateTime;
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void AddUltimateCharge(int amount)
        {
            _currentUltimateAmount += amount;
            _currentUltimateAmount = Mathf.Clamp(_currentUltimateAmount, 0, _maxUltimateChargeAmount);
        }

        #endregion External Functions

        #region Utils

        private void DisplayUltimateToHUD()
        {
            var ultimatePercent = (float)_currentUltimateAmount / _maxUltimateChargeAmount * PlayerStaticData.MaxUltimateDisplayLimit;
            HUD_PlayerAbilityDisplay.Instance.UpdateTimer(AbilityTrigger.Ultimate, $"{ultimatePercent:0.0} %", true);

            var overlayPercent = _currentUltimateAmount >= _maxUltimateChargeAmount ? 0 : 1;
            HUD_PlayerAbilityDisplay.Instance.UpdateOverlay(AbilityTrigger.Ultimate, overlayPercent);
            HUD_PlayerAbilityDisplay.Instance.UpdateCounter(AbilityTrigger.Ultimate, $"{_currentUltimateAmount}", _debugIsActive);
        }

        #endregion Utils
    }
}