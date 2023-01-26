#region

using Ability_Scripts.Projectiles;
using HealthSystem;
using Player.Base;
using Player.Common;
using UnityEngine;
using UnityEngine.Assertions;
using Utils.Misc;

#endregion

namespace Player.Type_1
{
    public class Type_1_Secondary_ChargedShoot : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _chargedObjectPrefab;
        [SerializeField] private GameObject _chargedObjectSpawnPrefab;

        [Header("Components")]
        [SerializeField] private BaseShootController _shootController;

        [Header("Charged Shoot Data")]
        [SerializeField] private float _maxChargeDuration;
        [SerializeField] private int _minChargeDamage;
        [SerializeField] private int _maxChargeDamage;

        private bool _abilityEnd;
        private float _currentChargeTime;

        private GameObject _chargedSpawnEffect;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var key = playerController.GetSecondaryAbilityKey();
            if (key.KeyReleasedThisFrame || !key.KeyPressed || _currentChargeTime >= _maxChargeDuration)
            {
                var spawnPosition = _shootController.GetShootPosition();
                var direction = _shootController.GetShootLookDirection();

                var projectile = Instantiate(_chargedObjectPrefab, spawnPosition, Quaternion.identity);
                var simpleProj = projectile.GetComponent<SimpleProjectile>();
                simpleProj.LaunchProjectile(direction);

                var mappedDamage = Mathf.CeilToInt(ExtensionFunctions.Map(_currentChargeTime, 0, _maxChargeDuration, _minChargeDamage, _maxChargeDamage));
                var simpleDamage = projectile.GetComponent<SimpleDamageOverrideTrigger>();
                simpleDamage.SetDamageAmount(mappedDamage);

                Assert.IsNotNull(_chargedSpawnEffect, "Charged Effect cannot be null here...");
                Destroy(_chargedSpawnEffect);

                _abilityEnd = true;
                _currentCooldownDuration = _cooldownDuration;
            }
            else
            {
                _currentChargeTime += Time.fixedDeltaTime;
            }
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            _abilityEnd = true;
            _currentChargeTime = 0;
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            _abilityEnd = false;
            _currentChargeTime = 0;

            var shootPoint = _shootController.GetShootPoint();
            _chargedSpawnEffect = Instantiate(_chargedObjectSpawnPrefab, shootPoint.position, Quaternion.identity);
            _chargedSpawnEffect.transform.SetParent(shootPoint);
        }

        #endregion Ability Functions
    }
}