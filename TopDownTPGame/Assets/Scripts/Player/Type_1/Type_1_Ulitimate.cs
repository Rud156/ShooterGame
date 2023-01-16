#region

using Player.Base;
using Player.Common;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

namespace Player.Type_1
{
    public class Type_1_Ulitimate : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _ultimatePulsePrefab;

        public override bool AbilityCanStart(BasePlayerController playerController) => _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => true;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            // Nothing to update here...
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            // Nothing to do here...
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            var characterTransform = transform;
            Instantiate(_ultimatePulsePrefab, characterTransform.position, Quaternion.identity, characterTransform);
            _currentCooldownDuration = _cooldownDuration;
        }
    }
}