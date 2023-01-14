#region

using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_1
{
    public class Type_1_Primary_SimpleShoot : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _projectilePrefab;

        [Header("Components")]
        [SerializeField] private BaseShootController _shootController;
        [SerializeField] private Transform _shootPoint;

        [Header("Simple Shoot Data")]
        [SerializeField] private float _fireRate;

        private float _nextShootTime;
        private bool _abilityEnd;

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (Time.time >= _nextShootTime)
            {
                _nextShootTime = Time.time + _fireRate;
                var spawnPosition = _shootPoint.position;
                var direction = _shootController.GetShootLookDirection();

                var projectile = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);
                var simpleProj = projectile.GetComponent<SimpleProjectile>();
                simpleProj.LaunchProjectile(direction);
            }

            var inputKey = playerController.GetPrimaryAbilityKey();
            if (inputKey.KeyReleasedThisFrame || !inputKey.KeyPressed)
            {
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;
    }
}