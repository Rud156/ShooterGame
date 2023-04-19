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
        [SerializeField] private float _ultimateChargeRate;

        private float _currentUltimatePercent;
        private GameObject _warCryObject;

        private float _currentWindUpTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) =>
            base.AbilityCanStart(playerController) && _currentUltimatePercent >= PlayerStaticData.MaxUltimatePercent;

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
            _currentUltimatePercent = 0;
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

            if (_currentUltimatePercent < PlayerStaticData.MaxUltimatePercent)
            {
                _currentUltimatePercent += WorldTimeManager.Instance.FixedUpdateTime * _ultimateChargeRate;
                if (_currentUltimatePercent > PlayerStaticData.MaxUltimatePercent)
                {
                    _currentUltimatePercent = PlayerStaticData.MaxUltimatePercent;
                }
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void AddUltimateCharge(int amount)
        {
            _currentUltimatePercent += amount;
            if (_currentUltimatePercent > PlayerStaticData.MaxUltimatePercent)
            {
                _currentUltimatePercent = PlayerStaticData.MaxUltimatePercent;
            }
        }

        #endregion External Functions

        #region Utils

        private void DisplayUltimateToHUD()
        {
            HUD_PlayerAbilityDisplay.Instance.UpdateTimer(AbilityTrigger.Ultimate, $"{_currentUltimatePercent:0.0} %", true);

            var overlayPercent = _currentUltimatePercent >= PlayerStaticData.MaxUltimatePercent ? 0 : 1;
            HUD_PlayerAbilityDisplay.Instance.UpdateOverlay(AbilityTrigger.Ultimate, overlayPercent);
        }

        #endregion Utils
    }
}