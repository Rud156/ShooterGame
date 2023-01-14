#region

using System.Collections.Generic;
using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Input;

#endregion

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
        private List<GameObject> _spawnedTurrets;

        #region Unity Functions

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            base.UnityStartDelegate(playerController);

            _spawnedTurrets = new List<GameObject>();
        }

        #endregion Unity Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => playerController.IsGrounded;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _placementCompleted;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var cameraPosition = _cameraPoint.position;
            var cameraForward = _cameraHolder.forward;
            var hit = Physics.Raycast(cameraPosition, cameraForward, out var hitInfo, _spawnMaxDistance, _turretMask);
            Debug.DrawRay(cameraPosition, cameraForward * _spawnMaxDistance, color: Color.red);
            PlayerInputKey primaryKey = playerController.GetPrimaryAbilityKey();
            if (hit)
            {
                // TODO: Do not spawn the turret if it spawn inside some geometry...

                _turretObject.transform.position = hitInfo.point + _spawnOffset;
                _turretObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

                if (primaryKey.KeyPressedThisFrame && _firstUpdateCompleted)
                {
                    _turretObject.transform.SetParent(hitInfo.transform);
                    _spawnedTurrets.Add(_turretObject);

                    _turretObject = null;
                    _placementCompleted = true;
                }
            }

            var secondaryKey = playerController.GetSecondaryAbilityKey();
            var tertiaryKey = playerController.GetTertiaryAbilityKey();
            var ultimateKey = playerController.GetUltimateAbilityKey();

            if (secondaryKey.KeyPressedThisFrame || tertiaryKey.KeyPressedThisFrame || ultimateKey.KeyPressedThisFrame)
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

        public override void ClearAllAbilityData(BasePlayerController playerController)
        {
            foreach (var turret in _spawnedTurrets)
            {
                Destroy(turret);
            }

            _spawnedTurrets.Clear();
        }
    }
}