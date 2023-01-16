#region

using Player.Base;
using UI;
using UnityEngine;
using Utils.Misc;

#endregion

namespace Player.Common
{
    public abstract class Ability : MonoBehaviour
    {
        private const float DefaultCooldownMultiplier = 1;

        [Header("Ability Display")]
        [SerializeField] protected Sprite _icon;
        [SerializeField] private Sprite _background;

        [Header("Core Ability Data")]
        [SerializeField] private AbilityTrigger _abilityTrigger;
        [SerializeField] private AbilityType _abilityType;

        [Header("Cooldown")]
        [SerializeField] protected float _cooldownDuration;

        protected float _currentCooldownDuration;
        protected float _cooldownMultiplier = DefaultCooldownMultiplier;

        #region Core Ability Functions

        public abstract void StartAbility(BasePlayerController playerController);

        public abstract void AbilityUpdate(BasePlayerController playerController);

        public abstract void EndAbility(BasePlayerController playerController);

        public abstract bool AbilityCanStart(BasePlayerController playerController);

        public abstract bool AbilityNeedsToEnd(BasePlayerController playerController);

        public virtual void ClearAllAbilityData(BasePlayerController playerController)
        {
        }

        public virtual void UnityStartDelegate(BasePlayerController playerController)
        {
            PlayerAbilityDisplay.Instance.UpdateAbilityTrigger(_abilityTrigger);
            PlayerAbilityDisplay.Instance.UpdateAbilityIcon(_abilityTrigger, _icon);
            PlayerAbilityDisplay.Instance.UpdateAbilityBackground(_abilityTrigger, _background, ExtensionFunctions.AverageColorFromTexture(_icon.texture));
        }

        public virtual void UnityUpdateDelegate(BasePlayerController playerController)
        {
        }

        public virtual void UnityFixedUpdateDelegate(BasePlayerController playerController)
        {
            if (_currentCooldownDuration > 0)
            {
                _currentCooldownDuration -= Time.fixedDeltaTime * _cooldownMultiplier;
                if (_currentCooldownDuration < 0)
                {
                    _currentCooldownDuration = 0;
                }

                PlayerAbilityDisplay.Instance.UpdateCooldownTimer(_abilityTrigger, _currentCooldownDuration, _currentCooldownDuration / _cooldownDuration);
            }
        }

        #endregion Core Ability Functions

        #region Cooldown Modifier Functions

        public void UpdateCooldownMultiplier(float cooldownMultiplier) => _cooldownMultiplier = cooldownMultiplier;

        public void FixedCooldownReduction(float amount)
        {
            _currentCooldownDuration -= amount;
            if (_currentCooldownDuration < 0)
            {
                _currentCooldownDuration = 0;
            }
        }

        public void PercentCooldownReduction(float percent)
        {
            var amount = _cooldownDuration * percent;
            _currentCooldownDuration -= amount;
            if (_currentCooldownDuration < 0)
            {
                _currentCooldownDuration = 0;
            }
        }

        #endregion Cooldown Modifier Functions

        #region Getters

        public AbilityType GetAbilityType() => _abilityType;

        public AbilityTrigger GetAbilityTrigger() => _abilityTrigger;

        #endregion Getters

        #region Specific Data

        public virtual Vector3 GetMovementData() => Vector3.zero;

        #endregion Specific Data
    }
}