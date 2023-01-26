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

        [Header("Simple Shoot Data")]
        [SerializeField] private float _fireRate;
        [SerializeField] private float _overheatTime;
        [SerializeField] private float _overheatCooldownMultiplier;

        private float _nextShootTime;
        private bool _abilityEnd;

        private float _currentOverheatTime;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (Time.time >= _nextShootTime)
            {
                _nextShootTime = Time.time + _fireRate;
                var spawnPosition = _shootController.GetShootPosition();
                var direction = _shootController.GetShootLookDirection();

                var projectile = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);
                var simpleProj = projectile.GetComponent<SimpleProjectile>();
                simpleProj.LaunchProjectile(direction);

                _currentOverheatTime += _fireRate;
            }

            if (_currentOverheatTime >= _overheatTime)
            {
                _currentCooldownDuration = _cooldownDuration;
                _currentOverheatTime = 0;
                _abilityEnd = true;
            }

            var inputKey = playerController.GetPrimaryAbilityKey();
            if (inputKey.KeyReleasedThisFrame || !inputKey.KeyPressed)
            {
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityFixedUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityFixedUpdateDelegate(playerController);

            if (_currentOverheatTime > 0 && _abilityEnd)
            {
                _currentOverheatTime -= Time.fixedDeltaTime * _overheatCooldownMultiplier;
            }
        }

        #endregion Unity Functions
    }
}