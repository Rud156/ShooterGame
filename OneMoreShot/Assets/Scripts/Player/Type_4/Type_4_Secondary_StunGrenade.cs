using CustomCamera;
using Player.Abilities;
using Player.Core;
using Projectiles;
using UI.Player;
using UnityEngine;
using Utils.Common;

namespace Player.Type_4
{
    public class Type_4_Secondary_StunGrenade : AbilityBase
    {
        private static readonly int Type_4_Secondary = Animator.StringToHash("Type_4_Secondary");

        [Header("Prefabs")]
        [SerializeField] private GameObject _stunGrenadePrefab;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _abilityCameraShaker;

        private bool _abilityMarkedForEnd;

        #region Core Ability Functions

        public override void AbilityStart(PlayerController playerController)
        {
            base.AbilityStart(playerController);
            _abilityMarkedForEnd = false;
        }

        public override void AbilityFixedUpdate(PlayerController playerController, float fixedDeltaTime)
        {
            var stunGrenade = Instantiate(_stunGrenadePrefab, _playerShootController.GetShootPosition(), Quaternion.identity);
            var projectile = stunGrenade.GetComponent<StunGrenade>();
            var ownerData = stunGrenade.GetComponent<OwnerData>();

            ownerData.OwnerId = _ownerData.OwnerId;
            projectile.LaunchProjectile(_playerShootController.GetShootLookDirection());

            _currentCooldownDuration = _abilityCooldownDuration;
            _abilityMarkedForEnd = true;

            CustomCameraController.Instance.StartShake(_abilityCameraShaker);
            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
        }

        public override void AbilityUpdate(PlayerController playerController, float deltaTime)
        {
        }

        public override void AbilityEnd(PlayerController playerController)
        {
        }

        #endregion Core Ability Functions

        #region Ability Conditions

        public override bool AbilityNeedsToEnd(PlayerController playerController) => _abilityMarkedForEnd;

        #endregion Ability Conditions
    }
}