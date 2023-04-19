#region

using Ability_Scripts.Projectiles;
using CustomCamera;
using Player.Base;
using Player.Common;
using UI.DisplayManagers.Player;
using UnityEngine;

#endregion

namespace Player.Type_4
{
    public class Type_4_Primary_PlasmaBomb : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _plasmaBombPrefab;

        [Header("Components")]
        [SerializeField] private PlayerBaseShootController _shootController;
        [SerializeField] private Type_4_DroneController _droneController;

        [Header("Plasma Shoot Data")]
        [SerializeField] private float _fireRate;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _cameraShaker;

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
                plasmaBomb.LaunchProjectile(direction);

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
    }
}