#region

using Ability_Scripts.Projectiles;
using CustomCamera;
using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;

#endregion

namespace Player.Type_5
{
    public class Type_5_Tertiary_TwoWayShield : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _shieldPrefab;

        [Header("Components")]
        [SerializeField] private PlayerBaseShootController _shootController;
        [SerializeField] private Animator _playerAnimator;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _cameraShaker;

        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var shieldObject = Instantiate(_shieldPrefab, _shootController.GetShootPosition(), Quaternion.identity);
            var shieldDeploy = shieldObject.GetComponent<ShieldDeployProjectile>();
            shieldDeploy.LaunchProjectile(_shootController.GetShootLookDirection());

            _currentCooldownDuration = _cooldownDuration;
            _abilityEnd = true;
            _playerAnimator.SetTrigger(PlayerStaticData.Type_5_Tertiary);

            CustomCameraController.Instance.StartShake(_cameraShaker);
            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;

        #endregion Ability Functions
    }
}