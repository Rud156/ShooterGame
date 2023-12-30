using Player.Core;
using System.Collections.Generic;
using UI.Player;
using UnityEngine;
using Utils.Common;
using Utils.Misc;

namespace Player.Abilities
{
    public abstract class AbilityBase : MonoBehaviour
    {
        private const float DefaultCooldownMultiplier = 1;

        [Header("Ability Display")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private Sprite _background;

        [Header("Core Ability Data")]
        [SerializeField] private bool _hasInputActivation;
        [SerializeField] private bool _isMovementAbility;
        [SerializeField] protected AbilityTrigger _abilityTrigger;
        [SerializeField] private AbilityType _abilityType;
        [SerializeField] protected float _abilityCooldownDuration;
        [SerializeField] private AbilitySpawnOrEffectPosition _abilityPositioning;
        [SerializeField] private List<AbilityType> _disallowedActiveAbilityTypes;

        private float _cooldownMultiplier = DefaultCooldownMultiplier;
        protected float _currentCooldownDuration;

        // Components (These will be overwritten everytime the Ability Starts)
        protected PlayerController _playerController;
        protected PlayerShootController _playerShootController;
        protected Animator _playerAnimator;
        protected OwnerData _ownerData;

        // Getters
        public AbilityType AbilityNameType => _abilityType;
        public bool IsMovementAbility => _isMovementAbility;

        public delegate void AbilityCooldownComplete();
        public event AbilityCooldownComplete OnAbilityCooldownComplete;

        #region Core Ability Functions

        // Ability initialization here...
        public virtual void AbilityStart(PlayerController playerController) => SetPlayerComponents(playerController);

        // Ability Updates
        public abstract void AbilityFixedUpdate(PlayerController playerController, float fixedDeltaTime);

        public abstract void AbilityUpdate(PlayerController playerController, float deltaTime);

        // Ability Data cleanup here...
        public abstract void AbilityEnd(PlayerController playerController);

        #endregion Core Ability Functions

        #region Ability Conditions

        public virtual bool AbilityCanStart(PlayerController playerController, bool ignoreCooldown = false)
        {
            if (_currentCooldownDuration > 0 && !ignoreCooldown)
            {
                return false;
            }

            bool canActivateAbility = false;
            if (_hasInputActivation)
            {
                var inputKey = playerController.GetKeyForAbilityTrigger(_abilityTrigger);
                if (inputKey.KeyPressedThisFrame)
                {
                    canActivateAbility = true;
                }
            }

            var abilities = playerController.ActiveAbilities;
            foreach (var ability in abilities)
            {
                var abilityType = ability.AbilityNameType;
                if (ability.IsMovementAbility && _isMovementAbility)
                {
                    // Basically cannot run more than 1 Movement Ability at once...
                    canActivateAbility = false;
                }

                if (!_disallowedActiveAbilityTypes.Contains(abilityType))
                {
                    canActivateAbility = false;
                }
            }

            return canActivateAbility;
        }

        public abstract bool AbilityNeedsToEnd(PlayerController playerController);

        #endregion Ability Conditions

        #region Getters

        public virtual Vector3 GetMovementData() => Vector3.zero;

        public bool HasAbilityNameInDisAllowedList(AbilityType activeAbilityAbilityNameType) => _disallowedActiveAbilityTypes.Contains(activeAbilityAbilityNameType);

        #endregion Getters

        #region Unity Function Delegates

        public virtual void UnityStartDelegate(PlayerController playerController)
        {
            SetPlayerComponents(playerController);
            if (_abilityTrigger != AbilityTrigger.ExternalAddedAbility)
            {
                HUD_PlayerAbilityDisplay.Instance.UpdateAbilityIcon(_abilityTrigger, _icon);
                HUD_PlayerAbilityDisplay.Instance.UpdateAbilityBackground(_abilityTrigger, _background, ExtensionFunctions.AverageColorFromTexture(_icon.texture));
            }
        }

        public virtual void UnityUpdateDelegate(PlayerController playerController)
        {
        }

        public virtual void UnityFixedUpdateDelegate(PlayerController playerController, float fixedDeltaTime)
        {
            if (_currentCooldownDuration > 0)
            {
                _currentCooldownDuration -= fixedDeltaTime * _cooldownMultiplier;
                if (_currentCooldownDuration <= 0)
                {
                    _currentCooldownDuration = 0;
                    OnAbilityCooldownComplete?.Invoke();
                }

                var showTimer = _currentCooldownDuration > 0;
                HUD_PlayerAbilityDisplay.Instance.UpdateTimer(_abilityTrigger, $"{_currentCooldownDuration:0.0}", showTimer);
                HUD_PlayerAbilityDisplay.Instance.UpdateOverlay(_abilityTrigger, _currentCooldownDuration / _abilityCooldownDuration);
            }
        }

        public virtual void UnityDestroyDelegate(PlayerController playerController)
        {
        }

        #endregion Unity Function Delegates

        #region Ability Cooldowns

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
            var amount = _abilityCooldownDuration * percent;
            _currentCooldownDuration -= amount;
            if (_currentCooldownDuration < 0)
            {
                _currentCooldownDuration = 0;
            }
        }

        #endregion Ability Cooldowns

        #region Misc

        private void SetPlayerComponents(PlayerController playerController)
        {
            _playerController = playerController;
            _playerAnimator = playerController.PlayerAnimator;
            _playerShootController = playerController.PlayerShootController;
            _ownerData = playerController.OwnerData;
        }

        public void ForceMarkAbilityAsMovement() => _isMovementAbility = true;

        #endregion Misc
    }
}