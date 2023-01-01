#region

using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_3
{
    public class Type_3_Ultimate_DarkPulse : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _darkPulsePrefab;

        private GameObject _darkPulseObject;
        private bool _abilityEnd;

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var darkPulse = Instantiate(_darkPulsePrefab, transform.position, Quaternion.identity);
            var pulse = darkPulse.GetComponent<DarkPulse>();
            pulse.StartPulse();

            _darkPulseObject = darkPulse;
            _abilityEnd = true;
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;

        public override void ClearAllAbilityData(BasePlayerController playerController)
        {
            if (_darkPulseObject != null)
            {
                Destroy(_darkPulseObject);
            }
        }
    }
}