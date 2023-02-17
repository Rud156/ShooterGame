#region

using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;
using Utils.Misc;

#endregion

namespace Player.Type_1
{
    public class Type_1_Ultimate_CooldownBooster : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _ultimatePulsePrefab;

        [Header("Ultimate Data")]
        [SerializeField] private float _windUptime;
        [SerializeField] private float _ultimateChargeRate;

        private float _currentUltimatePercent;
        private GameObject _kitsuneRushObject;

        private float _currentWindUpTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) =>
            base.AbilityCanStart(playerController) && _currentUltimatePercent >= StaticData.MaxUltimatePercent;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentWindUpTime -= Time.fixedDeltaTime;
            if (_currentWindUpTime <= 0)
            {
                var characterTransform = transform;
                _kitsuneRushObject = Instantiate(_ultimatePulsePrefab, characterTransform.position, Quaternion.identity, characterTransform);
                _abilityEnd = true;
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
            if (_kitsuneRushObject != null)
            {
                Destroy(_kitsuneRushObject);
                _kitsuneRushObject = null;
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

            if (_currentUltimatePercent < StaticData.MaxUltimatePercent)
            {
                _currentUltimatePercent += Time.fixedDeltaTime * _ultimateChargeRate;
                if (_currentUltimatePercent > StaticData.MaxUltimatePercent)
                {
                    _currentUltimatePercent = StaticData.MaxUltimatePercent;
                }
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void AddUltimateCharge(int amount)
        {
            _currentUltimatePercent += amount;
            if (_currentUltimatePercent > StaticData.MaxUltimatePercent)
            {
                _currentUltimatePercent = StaticData.MaxUltimatePercent;
            }
        }

        #endregion External Functions

        #region Utils

        private void DisplayUltimateToHUD()
        {
            HUD_PlayerAbilityDisplay.Instance.UpdateTimer(AbilityTrigger.Ultimate, $"{_currentUltimatePercent:0.0} %", true);

            var overlayPercent = _currentUltimatePercent >= StaticData.MaxUltimatePercent ? 0 : 1;
            HUD_PlayerAbilityDisplay.Instance.UpdateOverlay(AbilityTrigger.Ultimate, overlayPercent);
        }

        #endregion Utils
    }
}