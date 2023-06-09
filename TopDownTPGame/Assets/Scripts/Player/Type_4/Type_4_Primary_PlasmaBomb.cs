#region

using System;
using Ability_Scripts.Projectiles;
using CustomCamera;
using Player.Base;
using Player.Common;
using UI.DisplayManagers.Player;
using UnityEngine;
using Utils.Common;

#endregion

namespace Player.Type_4
{
    public class Type_4_Primary_PlasmaBomb : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _plasmaBombPrefab;

        [Header("Components")]
        [SerializeField] private GameObject _parent;
        [SerializeField] private PlayerBaseShootController _shootController;
        [SerializeField] private Type_4_DroneController _droneController;
        [SerializeField] private Type_4_Ultimate_Rocket _type4Ultimate;

        [Header("Plasma Shoot Data")]
        [SerializeField] private float _fireRate;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _cameraShaker;

        [Header("Ultimate Charge Data")]
        [SerializeField] private int _plasmaBombLineUltimateChargeAmount;
        [SerializeField] private int _plasmaPulseUltimateChargeAmount;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private float _nextShootTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (Time.time >= _nextShootTime)
            {
                _nextShootTime = Time.time + _fireRate;

                var spawnPosition = _shootController.GetShootPosition();
                var direction = _shootController.GetShootLookDirection();
                if (_debugIsActive)
                {
                    Debug.DrawRay(spawnPosition, direction * 50, Color.red, _debugDisplayDuration);
                }

                var projectile = Instantiate(_plasmaBombPrefab, spawnPosition, Quaternion.identity);
                var plasmaBomb = projectile.GetComponent<PlasmaBombLine>();
                var ownerData = projectile.GetComponent<OwnerData>();

                plasmaBomb.SetCollisionCallback(HandlePlasmaBombCollision);
                plasmaBomb.SetParentSpawner(this);
                plasmaBomb.LaunchProjectile(direction);
                ownerData.OwnerId = _parent.GetInstanceID();

                _droneController.KnockbackDrone(PlayerStaticData.Type_4_PrimaryDroneKnockbackMultiplier);
                CustomCameraController.Instance.StartShake(_cameraShaker);
                HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
            }

            var inputKey = playerController.GetKeyForAbilityTrigger(_abilityTrigger);
            if (inputKey.KeyReleasedThisFrame || !inputKey.KeyPressed)
            {
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;

        #endregion Ability Functions

        #region External Functions

        public void AddCallbackFunctionToPlasmaPulse(PlasmaPulse plasmaPulse) => plasmaPulse.SetCollisionCallback(HandlePlasmaBombCollision);

        private void HandlePlasmaBombCollision(Collider other, PlasmaBombType plasmaBombType)
        {
            switch (plasmaBombType)
            {
                case PlasmaBombType.PlasmaBomLine:
                    _type4Ultimate.AddUltimateCharge(_plasmaBombLineUltimateChargeAmount);
                    break;

                case PlasmaBombType.PlasmaPulse:
                    _type4Ultimate.AddUltimateCharge(_plasmaPulseUltimateChargeAmount);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(plasmaBombType), plasmaBombType, null);
            }
        }

        #endregion External Functions

        #region Enums

        public enum PlasmaBombType
        {
            PlasmaBomLine,
            PlasmaPulse,
        }

        #endregion Enums
    }
}