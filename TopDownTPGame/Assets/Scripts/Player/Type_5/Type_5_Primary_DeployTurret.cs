#region

using System;
using System.Collections.Generic;
using Ability_Scripts.Spawns;
using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;
using UnityEngine.Serialization;
using Utils.Materials;

#endregion

namespace Player.Type_5
{
    public class Type_5_Primary_DeployTurret : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _turretPrefab;

        [Header("Components")]
        [SerializeField] private PlayerBaseShootController _shootController;

        [Header("Spawn Data")]
        [SerializeField] private float _spawnMaxDistance;
        [SerializeField] private int _maxTurretsCanSpawn;
        [SerializeField] private float _minYNormalThreshold;
        [SerializeField] private Vector3 _spawnOffset;
        [SerializeField] private LayerMask _turretDeployCheckMask;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

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
            if (_turretObject != null)
            {
                Destroy(_turretObject);
            }
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
            _turretObject = Instantiate(_turretPrefab, transform.position, Quaternion.identity);
            _turretMaterialSwitcher = _turretObject.GetComponent<BaseMaterialSwitcher>();

            SetTurretState(TurretState.Placement);
        }

        private void UpdateTurretPlacement(BasePlayerController playerController)
        {
            var shootPosition = _shootController.GetShootPosition();
            var direction = _shootController.GetShootLookDirection(_turretDeployCheckMask);
            var hit = Physics.Raycast(shootPosition, direction, out var hitInfo, _spawnMaxDistance, _turretDeployCheckMask);
            if (_debugIsActive)
            {
                Debug.DrawRay(shootPosition, direction * _spawnMaxDistance, Color.red, _debugDisplayDuration);
            }

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

                        var turretController = _turretObject.GetComponent<Type_5_TurretController>();
                        turretController.SetTurretActiveState(true);
                        turretController.SetOwnerInstanceId(gameObject.GetInstanceID());

                        UpdateTurretMaterial(1);

                        // Delete Last Spawned Turret...
                        if (_spawnedTurrets.Count >= _maxTurretsCanSpawn)
                        {
                            var turret = _spawnedTurrets[0];
                            Destroy(turret);
                            _spawnedTurrets.RemoveAt(0);
                        }

                        _spawnedTurrets.Add(_turretObject);
                        _turretObject = null;
                        _currentCooldownDuration = _cooldownDuration;
                        SetTurretState(TurretState.Placed);
                        HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
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
                _turretObject.transform.position = shootPosition + endPosition;
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