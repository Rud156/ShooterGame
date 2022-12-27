using AbilityScripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Input;

namespace Player.Type_4
{
    public class Type_4_Primary_PlasmaBomb : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _plasmaBombPrefab;

        [Header("Components")]
        [SerializeField] private Transform _cameraHolder;
        [SerializeField] private Transform _shootPoint;

        [Header("Plasma Shoot Data")]
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

                Vector3 spawnPosition = _shootPoint.position;
                Vector3 direction = _cameraHolder.forward;

                GameObject projectile = Instantiate(_plasmaBombPrefab, spawnPosition, Quaternion.identity);
                PlasmaBombLine plasmaBomb = projectile.GetComponent<PlasmaBombLine>();
                plasmaBomb.LaunchProjectile(direction);
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