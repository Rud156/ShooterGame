#region

using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_5
{
    public class Type_5_Tertiary_TwoWayShield : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _shieldPrefab;

        [Header("Spawn Data")]
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Transform _cameraHolder;

        private bool _abilityEnd;

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController);

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var shieldObject = Instantiate(_shieldPrefab, _shootPoint.position, Quaternion.identity);
            var shieldDeploy = shieldObject.GetComponent<ShieldDeployProjectile>();
            shieldDeploy.LaunchProjectile(_cameraHolder.forward);
            _abilityEnd = true;
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;
    }
}