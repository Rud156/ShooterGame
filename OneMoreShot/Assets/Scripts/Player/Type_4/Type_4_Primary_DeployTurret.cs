using ObjectControllers;
using Player.Abilities;
using Player.Core;
using System;
using System.Collections.Generic;
using UI.Player;
using UnityEngine;
using Utils.Materials;

namespace Player.Type_4
{
    public class Type_4_Primary_DeployTurret : AbilityBase
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _turretPrefab;

        [Header("Spawn Data")]
        [SerializeField] private float _spawnMaxDistance;
        [SerializeField] private int _maxTurretsCanSpawn;
        [SerializeField] private float _raycastDownDistance;
        [SerializeField] private float _minYNormalThreshold;
        [SerializeField] private float _maxYNormalThreshold;
        [SerializeField] private LayerMask _turretDeployCheckMask;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private BaseMaterialSwitcher _turretMaterialSwitcher;
        private GameObject _turretObject;
        private List<Type_4_TurretController> _spawnedTurretControllers;

        private TurretState _turretState;

        #region Core Ability Functions

        public override void AbilityStart(PlayerController playerController)
        {
            base.AbilityStart(playerController);
            SetTurretState(TurretState.ShouldSpawn);
        }

        public override void AbilityFixedUpdate(PlayerController playerController, float fixedDeltaTime)
        {
            switch (_turretState)
            {
                case TurretState.ShouldSpawn:
                    UpdateTurretShouldSpawn();
                    break;

                case TurretState.Placement:
                    UpdateTurretPlacement();
                    break;

                case TurretState.Placed:
                case TurretState.Cancelled:
                    // Do nothing here..
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void AbilityUpdate(PlayerController playerController, float deltaTime)
        {
        }

        public override void AbilityEnd(PlayerController playerController)
        {
            if (_turretObject != null)
            {
                Destroy(_turretObject);
            }
        }

        #endregion Core Ability Functions

        #region Ability Conditions

        public override bool AbilityNeedsToEnd(PlayerController playerController) => _turretState is TurretState.Placed or TurretState.Cancelled;

        #endregion Ability Conditions

        #region Ability Turret Functions

        private void UpdateTurretShouldSpawn()
        {
            _turretObject = Instantiate(_turretPrefab, transform.position, Quaternion.identity);
            _turretMaterialSwitcher = _turretObject.GetComponent<BaseMaterialSwitcher>();

            SetTurretState(TurretState.Placement);
        }

        private void UpdateTurretPlacement()
        {
            var shootPosition = _playerShootController.GetShootPosition();
            var direction = _playerShootController.GetShootLookDirection(_turretDeployCheckMask);
            var hit = Physics.Raycast(shootPosition, direction, out var hitInfo, _spawnMaxDistance, _turretDeployCheckMask);
            if (_debugIsActive)
            {
                Debug.DrawRay(shootPosition, direction * _spawnMaxDistance, Color.red, _debugDisplayDuration);
            }

            var spawnPosition = shootPosition + transform.forward * _spawnMaxDistance;
            if (hit)
            {
                spawnPosition = hitInfo.point;
            }

            var hitDown = Physics.Raycast(spawnPosition, Vector3.down, out var hitInfoDown, _raycastDownDistance, _turretDeployCheckMask);
            if (hitDown)
            {
                _turretObject.transform.SetPositionAndRotation(hitInfoDown.point, Quaternion.FromToRotation(Vector3.up, hitInfoDown.normal));

                var yNormal = hitInfoDown.normal.y;
                if (IsNormalInAcceptedRange(yNormal))
                {
                    UpdateTurretMaterial(2);

                    var primaryKey = _playerController.GetKeyForAbilityTrigger(AbilityTrigger.Primary);
                    if (primaryKey.KeyPressedThisFrame)
                    {
                        // Delete Last Spawned Turret...
                        if (_spawnedTurretControllers.Count >= _maxTurretsCanSpawn)
                        {
                            _spawnedTurretControllers[0].DestroyTurret();
                            _spawnedTurretControllers.RemoveAt(0);
                        }

                        UpdateTurretMaterial(1); // Update the Material first since ActivateTurret uses the turret Material
                        var turretController = _turretObject.GetComponent<Type_4_TurretController>();
                        turretController.ActivateTurret();

                        _turretObject.transform.SetParent(hitInfo.transform);
                        _spawnedTurretControllers.Add(turretController);
                        _turretObject = null;
                        _currentCooldownDuration = _abilityCooldownDuration;

                        SetTurretState(TurretState.Placed);
                        HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
                    }
                }
                else
                {
                    var endPosition = direction * _spawnMaxDistance;
                    _turretObject.transform.position = shootPosition + endPosition;
                    UpdateTurretMaterial(3);
                }
            }
            else
            {
                UpdateTurretMaterial(3);
            }

            var secondaryKey = _playerController.GetKeyForAbilityTrigger(AbilityTrigger.Secondary);
            if (secondaryKey.KeyPressedThisFrame)
            {
                Destroy(_turretObject);
                SetTurretState(TurretState.Cancelled);
            }
        }

        private void UpdateTurretMaterial(int materialIndex) => _turretMaterialSwitcher.SwitchMaterial(materialIndex);

        private void SetTurretState(TurretState turretState) => _turretState = turretState;

        private bool IsNormalInAcceptedRange(float currentYNormal) => currentYNormal >= _minYNormalThreshold && currentYNormal <= _maxYNormalThreshold;

        #endregion Ability Turret Functions

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