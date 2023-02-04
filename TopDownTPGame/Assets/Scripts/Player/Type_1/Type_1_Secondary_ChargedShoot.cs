#region

using Ability_Scripts.Projectiles;
using HealthSystem;
using Player.Base;
using Player.Common;
using Player.UI;
using UnityEngine;
using Utils.Misc;

#endregion

namespace Player.Type_1
{
    public class Type_1_Secondary_ChargedShoot : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _chargedObjectPrefab;

        [Header("Components")]
        [SerializeField] private BaseShootController _shootController;
        [SerializeField] private Type_1_Primary_SimpleShoot _type1Primary;

        [Header("Charged Shoot Data")]
        [SerializeField] private float _windUpTime;
        [SerializeField] private float _minChargeDamage;
        [SerializeField] private float _maxChargeDamage;

        private float _currentWindUpTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentWindUpTime -= Time.fixedDeltaTime;
            if (_currentWindUpTime <= 0)
            {
                var spawnPosition = _shootController.GetShootPosition();
                var direction = _shootController.GetShootLookDirection();

                var projectile = Instantiate(_chargedObjectPrefab, spawnPosition, Quaternion.identity);
                var simpleProj = projectile.GetComponent<SimpleProjectile>();
                simpleProj.LaunchProjectile(direction);

                var chargeAmount = _type1Primary.GetCurrentChargeAmount();
                var maxChargeAmount = _type1Primary.GetMaxChargeAmount();
                var mappedDamage = Mathf.CeilToInt(ExtensionFunctions.Map(chargeAmount, 0, maxChargeAmount, _minChargeDamage, _maxChargeDamage));
                var simpleDamage = projectile.GetComponent<SimpleDamageOverrideTrigger>();
                simpleDamage.SetDamageAmount(mappedDamage);

                _type1Primary.UseStoredCharge(chargeAmount);
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController)
        {
            _abilityEnd = false;
            _currentWindUpTime = _windUpTime;
        }

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityUpdateDelegate(playerController);

            var chargeAmount = _type1Primary.GetCurrentChargeAmount();
            PlayerAbilityDisplay.Instance.UpdateStackCount(AbilityTrigger.Secondary, chargeAmount);
            PlayerAbilityDisplay.Instance.UpdateOverlayStatus(AbilityTrigger.Secondary, chargeAmount <= 0);
        }

        #endregion Unity Functions
    }
}