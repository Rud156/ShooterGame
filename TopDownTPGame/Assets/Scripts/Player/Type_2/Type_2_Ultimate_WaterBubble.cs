#region

using Player.Base;
using Player.Common;
using Player.UI;
using UnityEngine;
using Utils.Misc;

#endregion

namespace Player.Type_2
{
    public class Type_2_Ultimate_WaterBubble : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _waterBubblePrefab;

        [Header("Bubble Data")]
        [SerializeField] private float _abilityCastRadius;
        [SerializeField] private LayerMask _abilityMask;

        [Header("Ultimate Data")]
        [SerializeField] private float _windUpTime;
        [SerializeField] private float _ultimateChargeRate;

        private Collider[] _hitColliders = new Collider[StaticData.MaxCollidersCheck];

        private float _currentWindUpTime;
        private float _currentUltimatePercent;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) =>
            base.AbilityCanStart(playerController) && _currentUltimatePercent >= StaticData.MaxUltimatePercent;

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
                    var targetTransform = targetController.transform;
                    var position = targetTransform.position;
                    var waterBubble = Instantiate(_waterBubblePrefab, position, Quaternion.identity, targetTransform);
                    var waterBubbleFrozen = waterBubble.GetComponent<Type_2_Ultimate_WaterBubbleFrozen>();
                    targetController.CheckAndAddExternalAbility(waterBubbleFrozen);
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

        private void DisplayUltimateToHUD() => PlayerAbilityDisplay.Instance.UpdateCooldownPercent(AbilityTrigger.Ultimate, _currentUltimatePercent, StaticData.MaxUltimatePercent);

        #endregion Utils
    }
}