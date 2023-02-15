#region

using System.Collections.Generic;
using Player.Base;
using UI.Player;
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
        [SerializeField] protected AbilityTrigger _abilityTrigger;
        [SerializeField] protected AbilityType _abilityType;
        [SerializeField] protected AbilityNameType _abilityNameType;
        [SerializeField] protected List<AbilityNameType> _allowedActiveAbilities;

        [Header("Cooldown")]
        [SerializeField] protected float _cooldownDuration;

        protected float _currentCooldownDuration;
        protected float _cooldownMultiplier = DefaultCooldownMultiplier;

        public delegate void AbilityCooldownComplete();

        public event AbilityCooldownComplete OnAbilityCooldownComplete;

        #region Core Ability Functions

        #region Ability Functions

        public abstract void StartAbility(BasePlayerController playerController);

        public abstract void AbilityUpdate(BasePlayerController playerController);

        public abstract void EndAbility(BasePlayerController playerController);

        public virtual bool AbilityCanStart(BasePlayerController playerController)
        {
            var abilities = playerController.GetActiveAbilities();
            foreach (var ability in abilities)
            {
                var abilityType = ability.GetAbilityType();
                var abilityName = ability.GetAbilityNameType();

                // Basically cannot run more than 1 Movement Ability at once...
                if (abilityType == AbilityType.Movement && _abilityType == AbilityType.Movement)
                {
                    return false;
                }

                if (!_allowedActiveAbilities.Contains(abilityName))
                {
                    return false;
                }
            }

            return true;
        }

        public abstract bool AbilityNeedsToEnd(BasePlayerController playerController);

        public bool HasAbilityNameInAllowedList(AbilityNameType abilityNameType) => _allowedActiveAbilities.Contains(abilityNameType);

        public virtual void ClearAllAbilityData(BasePlayerController playerController)
        {
        }

        #endregion Ability Functions

        #region Unity Functions

        public virtual void UnityStartDelegate(BasePlayerController playerController)
        {
            if (_abilityTrigger != AbilityTrigger.ExternalAddedAbility)
            {
                HUD_PlayerAbilityDisplay.Instance.UpdateAbilityTrigger(_abilityTrigger);
                HUD_PlayerAbilityDisplay.Instance.UpdateAbilityIcon(_abilityTrigger, _icon);
                HUD_PlayerAbilityDisplay.Instance.UpdateAbilityBackground(_abilityTrigger, _background, ExtensionFunctions.AverageColorFromTexture(_icon.texture));
            }
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
                    OnAbilityCooldownComplete?.Invoke();
                }

                HUD_PlayerAbilityDisplay.Instance.UpdateCooldownTimer(_abilityTrigger, _currentCooldownDuration, _currentCooldownDuration / _cooldownDuration);
            }
        }

        #endregion Unity Functions

        #endregion Core Ability Functions

        #region Cooldown Modifier Functions

        public void ChangeCooldownMultiplier(float cooldownMultiplier) => _cooldownMultiplier = cooldownMultiplier;

        public void ResetCooldownMultiplier() => _cooldownMultiplier = DefaultCooldownMultiplier;

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

        public AbilityNameType GetAbilityNameType() => _abilityNameType;

        #endregion Getters

        #region Specific Data

        public virtual Vector3 GetMovementData() => Vector3.zero;

        #endregion Specific Data
    }
}