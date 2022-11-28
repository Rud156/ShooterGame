using Player.Base;
using UnityEngine;

namespace Player.Common
{
    public abstract class Ability : MonoBehaviour
    {
        [Header("Core Ability Data")]
        [SerializeField] private AbilityType abilityType;
        [SerializeField] private float abilityChargeUpTime;
        

        public abstract void StartAbility();

        public abstract Vector3 AbilityUpdate(BasePlayerController playerController);

        public abstract void EndAbility();

        public abstract bool AbilityCanStart();

        public abstract bool AbilityNeedsToEnd();

        public AbilityType GetAbilityType() => abilityType;
    }
}