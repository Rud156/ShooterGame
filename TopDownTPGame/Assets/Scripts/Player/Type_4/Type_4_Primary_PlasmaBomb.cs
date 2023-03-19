#region

using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;

#endregion

namespace Player.Type_4
{
    public class Type_4_Primary_PlasmaBomb : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _plasmaBombPrefab;

        [Header("Components")]
        [SerializeField] private PlayerBaseShootController _shootController;

        [Header("Plasma Shoot Data")]
        [SerializeField] private float _fireRate;

        private float _nextShootTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController);

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (Time.time >= _nextShootTime)
            {
                _nextShootTime = Time.time + _fireRate;

                var spawnPosition = _shootController.GetShootPosition();
                var direction = _shootController.GetShootLookDirection();

                var projectile = Instantiate(_plasmaBombPrefab, spawnPosition, Quaternion.identity);
                var plasmaBomb = projectile.GetComponent<PlasmaBombLine>();
                plasmaBomb.LaunchProjectile(direction);
                HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
            }

            var inputKey = playerController.GetKeyForAbilityTrigger(_abilityTrigger);
            if (inputKey.KeyReleasedThisFrame || !inputKey.KeyPressed)
            {
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;

        #endregion Ability Functions
    }
}