#region

using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;

#endregion

namespace Player.Type_5
{
    public class Type_5_Tertiary_TwoWayShield : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _shieldPrefab;

        [Header("Components")]
        [SerializeField] private PlayerBaseShootController _shootController;

        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var shieldObject = Instantiate(_shieldPrefab, _shootController.GetShootPosition(), Quaternion.identity);
            var shieldDeploy = shieldObject.GetComponent<ShieldDeployProjectile>();
            shieldDeploy.LaunchProjectile(_shootController.GetShootLookDirection());
            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);

            _currentCooldownDuration = _cooldownDuration;
            _abilityEnd = true;
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;

        #endregion Ability Functions
    }
}