using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Input;

namespace Player.Type_5
{
    public class Type_5_Primary_DeployTurret : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _turretPrefab;

        [Header("Components")]
        [SerializeField] private Transform _cameraHolder;
        [SerializeField] private Transform _cameraPoint;

        [Header("Spawn Data")]
        [SerializeField] private float _spawnMaxDistance;
        [SerializeField] private Vector3 _spawnOffset;
        [SerializeField] private LayerMask _turretMask;

        private bool _placementCompleted;
        private bool _firstUpdateCompleted;

        private GameObject _turretObject;

        public override bool AbilityCanStart(BasePlayerController playerController) => playerController.IsGrounded;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _placementCompleted;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            bool hit = Physics.Raycast(_cameraPoint.position, _cameraHolder.forward, out RaycastHit hitInfo, _spawnMaxDistance, _turretMask);
            Debug.DrawRay(_cameraPoint.position, _cameraHolder.forward * _spawnMaxDistance, color: Color.red);
            if (hit)
            {
                _turretObject.transform.position = hitInfo.point + _spawnOffset;
                _turretObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

                PlayerInputKey primaryKey = playerController.GetPrimaryAbilityKey();
                if (primaryKey.keyPressedThisFrame && _firstUpdateCompleted)
                {
                    _turretObject = null;
                    _placementCompleted = true;
                }
            }

            PlayerInputKey secondaryKey = playerController.GetSecondaryAbilityKey();
            PlayerInputKey tertiaryKey = playerController.GetTertiaryAbilityKey();
            PlayerInputKey ultimateKey = playerController.GetUltimateAbilityKey();

            if (secondaryKey.keyPressedThisFrame || tertiaryKey.keyPressedThisFrame || ultimateKey.keyPressedThisFrame)
            {
                Destroy(_turretObject);
                _placementCompleted = true;
            }

            _firstUpdateCompleted = true;
        }

        public override void EndAbility(BasePlayerController playerController) => _placementCompleted = true;

        public override void StartAbility(BasePlayerController playerController)
        {
            _placementCompleted = false;
            _firstUpdateCompleted = false;
            _turretObject = Instantiate(_turretPrefab, transform.position, Quaternion.identity);
        }
    }
}