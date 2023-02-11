#region

using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_5
{
    public class Type_5_Secondary_StunGrenade : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _stunGrenadePrefab;

        [Header("Components")]
        [SerializeField] private BaseShootController _shootController;

        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController);

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var stunGrenade = Instantiate(_stunGrenadePrefab, _shootController.GetShootPosition(), Quaternion.identity);
            var projectile = stunGrenade.GetComponent<StunGrenade>();
            projectile.LaunchProjectile(_shootController.GetShootLookDirection());
            _abilityEnd = true;
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;

        #endregion Ability Functions
    }
}