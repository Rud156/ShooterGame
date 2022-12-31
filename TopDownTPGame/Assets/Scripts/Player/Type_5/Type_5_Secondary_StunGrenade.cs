using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;

namespace Player.Type_5
{
    public class Type_5_Secondary_StunGrenade : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _stunGrenadePrefab;

        [Header("Components")]
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Transform _cameraHolder;

        private bool _abilityEnd;

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var stunGrenade = Instantiate(_stunGrenadePrefab, _shootPoint.position, Quaternion.identity);
            var projectile = stunGrenade.GetComponent<StunGrenade>();
            projectile.LaunchProjectile(_cameraHolder.forward);
            _abilityEnd = true;
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;
    }
}