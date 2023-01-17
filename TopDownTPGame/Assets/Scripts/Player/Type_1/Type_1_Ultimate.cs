#region

using Player.Base;
using Player.Common;
using UI;
using UnityEngine;

#endregion

namespace Player.Type_1
{
    public class Type_1_Ultimate : Ability
    {
        private const float MaxUltimatePercent = 100;

        [Header("Prefabs")]
        [SerializeField] private GameObject _ultimatePulsePrefab;

        [Header("Ultimate Data")]
        [SerializeField] private float _ultimateChargeRate;

        private float _currentUltimatePercent;
        private GameObject _kitsuneRushObject;

        public override bool AbilityCanStart(BasePlayerController playerController) => _currentUltimatePercent >= MaxUltimatePercent;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => true;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            // Nothing to update here...
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            // Nothing to do here...
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            var characterTransform = transform;
            _kitsuneRushObject = Instantiate(_ultimatePulsePrefab, characterTransform.position, Quaternion.identity, characterTransform);
            _currentUltimatePercent = 0;
        }

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

        public override void ClearAllAbilityData(BasePlayerController playerController)
        {
            if (_kitsuneRushObject != null)
            {
                Destroy(_kitsuneRushObject);
                _kitsuneRushObject = null;
            }
        }

        #region External Functions

        public void AddUltimateCharge(int amount)
        {
            _currentUltimatePercent += amount;
            if (_currentUltimatePercent > MaxUltimatePercent)
            {
                _currentUltimatePercent = MaxUltimatePercent;
            }
        }

        #endregion External Functions

        #region Utils

        private void DisplayUltimateToHUD() => PlayerAbilityDisplay.Instance.UpdateCooldownPercent(AbilityTrigger.Ultimate, _currentUltimatePercent);

        #endregion Utils
    }
}