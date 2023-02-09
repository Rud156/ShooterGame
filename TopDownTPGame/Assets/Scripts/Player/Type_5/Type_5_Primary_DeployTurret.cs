#region

using System;
using System.Collections.Generic;
using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Materials;

#endregion

namespace Player.Type_5
{
    public class Type_5_Primary_DeployTurret : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _turretPrefab;

        [Header("Components")]
        [SerializeField] private Transform _camera;

        [Header("Spawn Data")]
        [SerializeField] private float _spawnMaxDistance;
        [SerializeField] private int _maxTurretsCanSpawn;
        [SerializeField] private float _minYNormalThreshold;
        [SerializeField] private Vector3 _spawnOffset;
        [SerializeField] private LayerMask _turretMask;

        private BaseMaterialSwitcher _turretMaterialSwitcher;
        private GameObject _turretObject;
        private List<GameObject> _spawnedTurrets;

        private TurretState _turretState;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _turretState is TurretState.Placed or TurretState.Cancelled;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            switch (_turretState)
            {
                case TurretState.ShouldSpawn:
                    UpdateTurretShouldSpawn();
                    break;

                case TurretState.Placement:
                    UpdateTurretPlacement(playerController);
                    break;

                case TurretState.Placed:
                case TurretState.Cancelled:
                    // Do nothing here..
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            // Do nothing here...
        }

        public override void StartAbility(BasePlayerController playerController) => SetTurretState(TurretState.ShouldSpawn);

        public override void ClearAllAbilityData(BasePlayerController playerController)
        {
            foreach (var turret in _spawnedTurrets)
            {
                Destroy(turret);
            }

            _spawnedTurrets.Clear();
        }

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            base.UnityStartDelegate(playerController);
            _spawnedTurrets = new List<GameObject>();
        }

        #endregion Unity Functions

        #region Utils

        #region Turret State Updates

        private void UpdateTurretShouldSpawn()
        {
            if (_spawnedTurrets.Count >= _maxTurretsCanSpawn)
            {
                var turret = _spawnedTurrets[0];
                Destroy(turret);
                _spawnedTurrets.RemoveAt(0);
            }

            _turretObject = Instantiate(_turretPrefab, transform.position, Quaternion.identity);
            _turretMaterialSwitcher = _turretObject.GetComponent<BaseMaterialSwitcher>();
            SetTurretState(TurretState.Placement);
        }

        private void UpdateTurretPlacement(BasePlayerController playerController)
        {
            var cameraPosition = _camera.position;
            var direction = _camera.forward;
            var hit = Physics.Raycast(cameraPosition, direction, out var hitInfo, _spawnMaxDistance, _turretMask);

            if (hit)
            {
                _turretObject.transform.position = hitInfo.point + _spawnOffset;
                _turretObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

                var yNormal = hitInfo.normal.y;
                if (IsNormalInAcceptedRange(yNormal))
                {
                    UpdateTurretMaterial(2);

                    var primaryKey = playerController.GetKeyForAbilityTrigger(_abilityTrigger);
                    if (primaryKey.KeyPressedThisFrame)
                    {
                        _turretObject.transform.SetParent(hitInfo.transform);
                        UpdateTurretMaterial(1);

                        _spawnedTurrets.Add(_turretObject);
                        _turretObject = null;
                        _currentCooldownDuration = _cooldownDuration;
                        SetTurretState(TurretState.Placed);
                    }
                }
                else
                {
                    UpdateTurretMaterial(3);
                }
            }
            else
            {
                var endPosition = direction * _spawnMaxDistance;
                _turretObject.transform.position = cameraPosition + endPosition;
                UpdateTurretMaterial(3);
            }

            var secondaryKey = playerController.GetKeyForAbilityTrigger(AbilityTrigger.Secondary);
            var tertiaryKey = playerController.GetKeyForAbilityTrigger(AbilityTrigger.Tertiary);
            var ultimateKey = playerController.GetKeyForAbilityTrigger(AbilityTrigger.Ultimate);

            if (secondaryKey.KeyPressedThisFrame || tertiaryKey.KeyPressedThisFrame || ultimateKey.KeyPressedThisFrame)
            {
                Destroy(_turretObject);
                SetTurretState(TurretState.Cancelled);
            }
        }

        #endregion Turret State Updates

        private bool IsNormalInAcceptedRange(float currentYNormal) => currentYNormal >= _minYNormalThreshold && currentYNormal <= 1;

        private void UpdateTurretMaterial(int materialIndex) => _turretMaterialSwitcher.SwitchMaterial(materialIndex);

        private void SetTurretState(TurretState turretState) => _turretState = turretState;

        #endregion Utils

        #region Enums

        private enum TurretState
        {
            ShouldSpawn,
            Placement,
            Placed,
            Cancelled,
        }

        #endregion Enums
    }
}