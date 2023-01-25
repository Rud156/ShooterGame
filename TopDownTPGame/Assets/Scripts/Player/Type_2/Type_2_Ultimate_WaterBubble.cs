#region

using Player.Base;
using Player.Common;
using UI;
using UnityEngine;

#endregion

namespace Player.Type_2
{
    public class Type_2_Ultimate_WaterBubble : Ability
    {
        private const int MaxCollidersCheck = 10;
        private const float MaxUltimatePercent = 100;

        [Header("Bubble Data")]
        [SerializeField] private float _abilityCastRadius;
        [SerializeField] private LayerMask _abilityMask;
        [SerializeField] private float _freezeDuration;

        [Header("Ultimate Data")]
        [SerializeField] private float _windUpTime;
        [SerializeField] private float _ultimateChargeRate;

        private Collider[] _hitColliders = new Collider[MaxCollidersCheck];

        private float _currentWindUpTime;
        private float _currentUltimatePercent;
        private bool _abilityEnd;

        public override bool AbilityCanStart(BasePlayerController playerController) => _currentUltimatePercent >= MaxUltimatePercent;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentWindUpTime -= Time.fixedDeltaTime;
            if (_currentWindUpTime > 0)
            {
                return;
            }

            var castPosition = transform.position;
            var totalHitColliders = Physics.OverlapSphereNonAlloc(castPosition, _abilityCastRadius, _hitColliders, _abilityMask);
            for (var i = 0; i < totalHitColliders; i++)
            {
                // Do not target itself
                if (_hitColliders[i] == null || _hitColliders[i].gameObject.GetInstanceID() == gameObject.GetInstanceID())
                {
                    continue;
                }

                if (_hitColliders[i].TryGetComponent(out BasePlayerController targetController))
                {
                    targetController.FreezeCharacter(_freezeDuration);
                }
            }

            _currentUltimatePercent = 0;
            _abilityEnd = true;
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController)
        {
            _abilityEnd = false;
            _currentWindUpTime = _windUpTime;
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

        private void DisplayUltimateToHUD() => PlayerAbilityDisplay.Instance.UpdateCooldownPercent(AbilityTrigger.Ultimate, _currentUltimatePercent, MaxUltimatePercent);

        #endregion Utils
    }
}