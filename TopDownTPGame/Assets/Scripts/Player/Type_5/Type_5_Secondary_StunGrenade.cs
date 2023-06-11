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

namespace Player.Type_5
{
    public class Type_5_Secondary_StunGrenade : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _stunGrenadePrefab;

        [Header("Components")]
        [SerializeField] private OwnerData _ownerIdData;
        [SerializeField] private PlayerBaseShootController _shootController;
        [SerializeField] private Type_5_Ultimate_ShieldAbility _type5Ultimate;
        [SerializeField] private Animator _playerAnimator;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _cameraShaker;

        [Header("Ultimate Charge Data")]
        [SerializeField] private int _primaryUltimateChargeAmount;
        [SerializeField] private int _secondaryUltimateChargeAmount;

        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var stunGrenade = Instantiate(_stunGrenadePrefab, _shootController.GetShootPosition(), Quaternion.identity);
            var projectile = stunGrenade.GetComponent<StunGrenade>();
            var ownerData = stunGrenade.GetComponent<OwnerData>();

            projectile.SetCollisionCallback(HandleStunGrenadeCollision);
            projectile.SetParentSpawner(this);
            projectile.LaunchProjectile(_shootController.GetShootLookDirection());
            ownerData.OwnerId = _ownerIdData.OwnerId;

            _currentCooldownDuration = _cooldownDuration;
            _abilityEnd = true;
            _playerAnimator.SetTrigger(PlayerStaticData.Type_5_Tertiary);

            CustomCameraController.Instance.StartShake(_cameraShaker);
            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;

        #endregion Ability Functions

        #region External Functions

        public void AddCallbackFunctionToSecondaryGrenade(StunGrenade stunGrenade) => stunGrenade.SetCollisionCallback(HandleStunGrenadeCollision);

        private void HandleStunGrenadeCollision(Collider other, StunGrenadeType stunGrenadeType)
        {
            switch (stunGrenadeType)
            {
                case StunGrenadeType.Primary:
                    _type5Ultimate.AddUltimateCharge(_primaryUltimateChargeAmount);
                    break;

                case StunGrenadeType.Secondary:
                    _type5Ultimate.AddUltimateCharge(_secondaryUltimateChargeAmount);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(stunGrenadeType), stunGrenadeType, null);
            }
        }

        #endregion External Functions

        #region Enums

        public enum StunGrenadeType
        {
            Primary,
            Secondary
        }

        #endregion Enums
    }
}