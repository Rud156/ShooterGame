using Player.Base;
using Player.Common;
using Projectiles;
using UnityEngine;
using Utils.Input;

namespace Player.Type_1
{
    public class Type_1_SimpleShoot : Ability
    {
        [Header("Simple Shoot Data")]
        [SerializeField] private float fireRate;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private GameObject projectilePrefab;

        private float _lastShootTime;
        private bool _abilityEnd;

        public override bool AbilityCanStart() => true;

        public override bool AbilityNeedsToEnd() => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (_lastShootTime <= 0)
            {
                _lastShootTime = fireRate;
                Vector3 spawnPosition = shootPoint.position;
                Vector3 direction = transform.forward;

                GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                SimpleOneShotForwardProjectile simpleProj = projectile.GetComponent<SimpleOneShotForwardProjectile>();
                simpleProj.LaunchProjectile(direction);
            }

            _lastShootTime -= Time.fixedDeltaTime;

            PlayerInputKey inputKey = playerController.GetKeyForAbilityTrigger(GetAbilityTrigger());
            if (inputKey.keyReleasedThisFrame || !inputKey.keyPressed)
            {
                _abilityEnd = true;
            }
        }

        public override void EndAbility()
        {
            _lastShootTime = 0;
            _abilityEnd = true;
        }

        public override void StartAbility()
        {
            _lastShootTime = 0;
            _abilityEnd = false;
        }
    }
}