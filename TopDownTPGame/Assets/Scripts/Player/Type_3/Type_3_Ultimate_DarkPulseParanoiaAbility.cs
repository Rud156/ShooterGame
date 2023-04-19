#region

using Player.Base;
using Player.Common;
using UI.DisplayManagers.Player;
using UnityEngine;
using World;

#endregion

namespace Player.Type_3
{
    public class Type_3_Ultimate_DarkPulseParanoiaAbility : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _darkPulsePrefab;

        [Header("Components")]
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private GameObject _parent;

        [Header("Ultimate Data")]
        [SerializeField] private float _windUpTime;
        [SerializeField] private float _ultimateChargeRate;

        private float _currentUltimatePercent;
        private GameObject _darkPulseObject;

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
                var darkPulse = Instantiate(_darkPulsePrefab, characterTransform.position, Quaternion.identity, characterTransform);

                var darkObject = darkPulse.GetComponent<Type_3_Ultimate_DarkPulseParanoia>();
                darkObject.SetOwnerInstanceId(_parent.GetInstanceID(), _playerAnimator, PlayerStaticData.Type_3_Ultimate);

                _darkPulseObject = darkPulse;
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController)
        {
            _currentWindUpTime = _windUpTime;
            _currentUltimatePercent = 0;
            _abilityEnd = false;

            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
        }

        public override void ClearAllAbilityData(BasePlayerController playerController)
        {
            if (_darkPulseObject != null)
            {
                Destroy(_darkPulseObject);
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