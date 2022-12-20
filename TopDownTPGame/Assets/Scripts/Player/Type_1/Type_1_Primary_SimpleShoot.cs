using AbilityScripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Input;

namespace Player.Type_1
{
    public class Type_1_Primary_SimpleShoot : Ability
    {
        [Header("Simple Shoot Data")]
        [SerializeField] private float _fireRate;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private GameObject _projectilePrefab;

        private float _lastShootTime;
        private bool _abilityEnd;

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (_lastShootTime <= 0)
            {
                _lastShootTime = _fireRate;
                Vector3 spawnPosition = _shootPoint.position;
                Vector3 direction = transform.forward;

                GameObject projectile = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);
                SimpleOneShotForwardProjectile simpleProj = projectile.GetComponent<SimpleOneShotForwardProjectile>();
                simpleProj.LaunchProjectile(direction);
            }

            _lastShootTime -= Time.fixedDeltaTime;

            PlayerInputKey inputKey = playerController.GetPrimaryAbilityKey();
            if (inputKey.keyReleasedThisFrame || !inputKey.keyPressed)
            {
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            _lastShootTime = 0;
            _abilityEnd = true;
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            _lastShootTime = 0;
            _abilityEnd = false;
        }
    }
}