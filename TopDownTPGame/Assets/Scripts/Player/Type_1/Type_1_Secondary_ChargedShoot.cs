using Player.Base;
using Player.Common;
using Projectiles;
using UnityEngine;
using Utils.Input;

namespace Player.Type_1
{
    public class Type_1_Secondary_ChargedShoot : Ability
    {
        [Header("Charged Shoot Data")]
        [SerializeField] private float _minChargeDuration;
        [SerializeField] private float _maxChargeDuration;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private GameObject _chargedObjectPrefab;

        private bool _abilityEnd;
        private float _currentChargeTime;

        public override bool AbilityCanStart() => true;

        public override bool AbilityNeedsToEnd() => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            PlayerInputKey key = playerController.GetKeyForAbilityTrigger(GetAbilityTrigger());
            if (key.keyReleasedThisFrame || !key.keyPressed || _currentChargeTime >= _maxChargeDuration)
            {
                if (_currentChargeTime >= _minChargeDuration)
                {
                    Vector3 spawnPosition = _shootPoint.position;
                    Vector3 direction = transform.forward;

                    GameObject projectile = Instantiate(_chargedObjectPrefab, spawnPosition, Quaternion.identity);
                    SimpleOneShotForwardProjectile simpleProj = projectile.GetComponent<SimpleOneShotForwardProjectile>();
                    simpleProj.LaunchProjectile(direction);
                }

                _abilityEnd = true;
            }
            else
            {
                _currentChargeTime += Time.fixedDeltaTime;
            }
        }

        public override void EndAbility()
        {
            _abilityEnd = true;
            _currentChargeTime = 0;
        }

        public override void StartAbility()
        {
            _abilityEnd = false;
            _currentChargeTime = 0;
        }
    }
}