#region

using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_1
{
    public class Type_1_Secondary_ChargedShoot : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _chargedObjectPrefab;

        [Header("Components")]
        [SerializeField] private BaseShootController _shootController;

        [Header("Charged Shoot Data")]
        [SerializeField] private float _minChargeDuration;
        [SerializeField] private float _maxChargeDuration;
        [SerializeField] private Transform _shootPoint;

        private bool _abilityEnd;
        private float _currentChargeTime;

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var key = playerController.GetSecondaryAbilityKey();
            if (key.KeyReleasedThisFrame || !key.KeyPressed || _currentChargeTime >= _maxChargeDuration)
            {
                if (_currentChargeTime >= _minChargeDuration)
                {
                    var spawnPosition = _shootPoint.position;
                    var direction = _shootController.GetShootLookDirection();

                    var projectile = Instantiate(_chargedObjectPrefab, spawnPosition, Quaternion.identity);
                    var simpleProj = projectile.GetComponent<SimpleProjectile>();
                    simpleProj.LaunchProjectile(direction);
                }

                _abilityEnd = true;
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
        }
    }
}