using AbilityScripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Input;

namespace Player.Type_1
{
    public class Type_1_Primary_SimpleShoot : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _projectilePrefab;

        [Header("Simple Shoot Data")]
        [SerializeField] private float _fireRate;
        [SerializeField] private Transform _shootPoint;

        private float _nextFireTime;
        private bool _abilityEnd;

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (Time.time >= _nextFireTime)
            {
                _nextFireTime = Time.time + _fireRate;
                Vector3 spawnPosition = _shootPoint.position;
                Vector3 direction = transform.forward;

                GameObject projectile = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);
                SimpleOneShotForwardProjectile simpleProj = projectile.GetComponent<SimpleOneShotForwardProjectile>();
                simpleProj.LaunchProjectile(direction);
            }

            PlayerInputKey inputKey = playerController.GetPrimaryAbilityKey();
            if (inputKey.keyReleasedThisFrame || !inputKey.keyPressed)
            {
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;
    }
}