using AbilityScripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;

namespace Player.Type_3
{
    public class Type_3_Ultimate_DarkPulse : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _darkPulsePrefab;

        private GameObject _darkPulseObject;

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => true;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
        }

        public override void EndAbility(BasePlayerController playerController)
        {
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            GameObject darkPulse = Instantiate(_darkPulsePrefab, transform.position, Quaternion.identity);
            DarkPulse pulse = darkPulse.GetComponent<DarkPulse>();
            pulse.StartPulse();

            _darkPulseObject = darkPulse;
        }

        public override void ClearAllAbilityData(BasePlayerController playerController)
        {
            if (_darkPulseObject != null)
            {
                Destroy(_darkPulseObject);
            }
        }
    }
}