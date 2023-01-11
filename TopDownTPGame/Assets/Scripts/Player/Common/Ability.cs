#region

using Player.Base;
using UI;
using UnityEngine;

#endregion

namespace Player.Common
{
    public abstract class Ability : MonoBehaviour
    {
        [Header("Ability Display")]
        [SerializeField] protected Sprite _icon;
        [SerializeField] private Sprite _background;

        [Header("Core Ability Data")]
        [SerializeField] private AbilityTrigger _abilityTrigger;
        [SerializeField] private AbilityType _abilityType;

        #region Unity Functions

        protected virtual void Start()
        {
            PlayerAbilityDisplay.Instance.UpdateAbilityIcon(_abilityTrigger, _icon);
            PlayerAbilityDisplay.Instance.UpdateAbilityBackground(_abilityTrigger, _background);
        }

        #endregion Unity Functions

        #region Core Ability Functions

        public abstract void StartAbility(BasePlayerController playerController);

        public abstract void AbilityUpdate(BasePlayerController playerController);

        public abstract void EndAbility(BasePlayerController playerController);

        public abstract bool AbilityCanStart(BasePlayerController playerController);

        public abstract bool AbilityNeedsToEnd(BasePlayerController playerController);

        public virtual void ClearAllAbilityData(BasePlayerController playerController)
        {
        }

        #endregion Core Ability Functions

        #region Getters

        public AbilityType GetAbilityType() => _abilityType;

        public AbilityTrigger GetAbilityTrigger() => _abilityTrigger;

        #endregion Getters

        #region Specific Data

        public virtual Vector3 GetMovementData() => Vector3.zero;

        #endregion Specific Data
    }
}