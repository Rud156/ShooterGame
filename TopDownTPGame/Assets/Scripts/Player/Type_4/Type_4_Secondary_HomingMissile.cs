using AbilityScripts.Projectiles;
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

                bool hit = Physics.Raycast(_cameraPoint.position, _cameraHolder.forward, out RaycastHit hitInfo, _targetDistance, _targetMask);
                Debug.DrawRay(_cameraPoint.position, _cameraHolder.forward, Color.red, 10);
                if (hit)
                {
                    DebugExtension.DebugWireSphere(_cameraPoint.position, 1, 10);
                    DebugExtension.DebugWireSphere(hitInfo.point, 1, 10);
                    if (hitInfo.collider.gameObject.TryGetComponent(out BasePlayerController targetController))
                    {
                        // Do something here...
                    }

                    GameObject missile = Instantiate(_homingMissilePrefab, _cameraPoint.position, Quaternion.identity);
                    PlasmaHomingMissile homingTarget = missile.GetComponent<PlasmaHomingMissile>();

                    Transform target = hitInfo.collider.transform;
                    homingTarget.SetTarget(target);
                }
            }

            PlayerInputKey inputKey = playerController.GetSecondaryAbilityKey();
            if (inputKey.keyReleasedThisFrame || !inputKey.keyPressed)
            {
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;
    }
}