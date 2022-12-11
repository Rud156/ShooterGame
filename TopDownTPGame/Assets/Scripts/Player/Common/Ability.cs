using Player.Base;
using UnityEngine;

namespace Player.Common
{
    public abstract class Ability : MonoBehaviour
    {
        [Header("Core Ability Data")]
        [SerializeField] private AbilityTrigger _abilityTrigger;
        [SerializeField] private AbilityType _abilityType;

        #region Core Ability Functions

        public abstract void StartAbility();

        public abstract void AbilityUpdate(BasePlayerController playerController);

        public abstract void EndAbility();

        public abstract bool AbilityCanStart();

        public abstract bool AbilityNeedsToEnd();

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