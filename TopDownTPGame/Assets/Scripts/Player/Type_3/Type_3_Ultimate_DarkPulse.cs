#region

using Player.Base;
using Player.Common;
using Player.UI;
using UnityEngine;

#endregion

namespace Player.Type_3
{
    public class Type_3_Ultimate_DarkPulse : Ability
    {
        private const float MaxUltimatePercent = 100;

        [Header("Prefabs")]
        [SerializeField] private GameObject _darkPulsePrefab;

        [Header("Ultimate Data")]
        [SerializeField] private float _windUpTime;
        [SerializeField] private float _ultimateChargeRate;

        private float _currentUltimatePercent;
        private GameObject _darkPulseObject;

        private float _currentWindUpTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => _currentUltimatePercent >= MaxUltimatePercent;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentWindUpTime -= Time.fixedDeltaTime;
            if (_currentWindUpTime <= 0)
            {
                var characterTransform = transform;
                var darkPulse = Instantiate(_darkPulsePrefab, characterTransform.position, Quaternion.identity, characterTransform);

                var darkObject = darkPulse.GetComponent<DarkPulse>();
                darkObject.SetOwnerInstanceId(gameObject.GetInstanceID());

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

            if (_currentUltimatePercent < MaxUltimatePercent)
            {
                _currentUltimatePercent += Time.fixedDeltaTime * _ultimateChargeRate;
                if (_currentUltimatePercent > MaxUltimatePercent)
                {
                    _currentUltimatePercent = MaxUltimatePercent;
                }
            }
        }

        #endregion Unity Functions

        #region Utils

        private void DisplayUltimateToHUD() => PlayerAbilityDisplay.Instance.UpdateCooldownPercent(AbilityTrigger.Ultimate, _currentUltimatePercent, MaxUltimatePercent);

        #endregion Utils
    }
}