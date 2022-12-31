using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Input;

namespace Player.Type_4
{
    public class Type_4_Secondary_HomingMissile : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _homingMissilePrefab;

        [Header("Components")]
        [SerializeField] private Transform _cameraHolder;
        [SerializeField] private Transform _cameraPoint;

        [Header("Spawn Data")]
        [SerializeField] private float _fireRate;
        [SerializeField] private float _targetDistance;
        [SerializeField] private LayerMask _targetMask;

        private float _nextShootTime;
        private bool _abilityEnd;

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (Time.time > _nextShootTime)
            {
                _nextShootTime = Time.time + _fireRate;

                var hit = Physics.Raycast(_cameraPoint.position, _cameraHolder.forward, out var hitInfo, _targetDistance, _targetMask);
                if (hit)
                {
                    if (hitInfo.collider.gameObject.TryGetComponent(out BasePlayerController targetController))
                    {
                        // Do something here...
                    }

                    var missile = Instantiate(_homingMissilePrefab, _cameraPoint.position, Quaternion.identity);
                    var homingTarget = missile.GetComponent<PlasmaHomingMissile>();

                    var target = hitInfo.collider.transform;
                    homingTarget.SetTarget(target);
                }
            }

            var inputKey = playerController.GetSecondaryAbilityKey();
            if (inputKey.KeyReleasedThisFrame || !inputKey.KeyPressed)
            {
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;
    }
}