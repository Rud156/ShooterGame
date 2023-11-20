using System.Collections.Generic;
using Player.Core;
using UnityEngine;

namespace Player.Abilities
{
    public abstract class AbilityBase : MonoBehaviour
    {
        [Header("Ability Display")]
        [SerializeField] protected Sprite _icon;
        [SerializeField] private Sprite _background;

        [Header("Core Ability Data")]
        [SerializeField] protected AbilityTrigger _abilityTrigger;
        [SerializeField] protected AbilityType _abilityType;
        [SerializeField] protected AbilitySpawnOrEffectPosition _abilityPositioning;
        [SerializeField] protected List<AbilityType> _allowedActiveAbilities;

        #region Core Ability Functions

        // Ability initialization here...
        public abstract void AbilityStart(PlayerController playerController);

        // Ability Updates
        public abstract void AbilityFixedUpdate(PlayerController playerController, float fixedDeltaTime);
        public abstract void AbilityUpdate(PlayerController playerController, float deltaTime);

        // Ability Data cleanup here...
        public abstract void AbilityEnd(PlayerController playerController);

        #endregion

        #region Ability Conditions

        public abstract void AbilityCanStart(PlayerController playerController);
        public abstract void AbilityNeedsToEnd(PlayerController playerController);

        #endregion

        #region Unity Delegate Updates

        public virtual void UnityStartDelegate(PlayerController playerController)
        {
        }

        public virtual void UnityUpdateDelegate(PlayerController playerController)
        {
        }

        public virtual void UnityFixedUpdateDelegate(PlayerController playerController)
        {
        }

        #endregion
    }
}