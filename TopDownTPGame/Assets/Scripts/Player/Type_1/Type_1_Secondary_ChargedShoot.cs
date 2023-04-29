#region

using Ability_Scripts.Projectiles;
using CustomCamera;
using HealthSystem;
using Player.Base;
using Player.Common;
using UI.DisplayManagers.Player;
using UnityEngine;
using Utils.Misc;
using World;

#endregion

namespace Player.Type_1
{
    public class Type_1_Secondary_ChargedShoot : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _chargedObjectPrefab;

        [Header("Components")]
        [SerializeField] private PlayerBaseShootController _shootController;
        [SerializeField] private Type_1_Primary_SimpleShoot _type1Primary;
        [SerializeField] private Type_1_Ultimate_WarCryPulseAbility _type1Ultimate;
        [SerializeField] private Animator _playerAnimator;

        [Header("Charged Shoot Data")]
        [SerializeField] private float _windUpTime;
        [SerializeField] private float _minChargeDamage;
        [SerializeField] private float _maxChargeDamage;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _cameraShaker;

        [Header("Animations")]
        [SerializeField] private int _attackAnimCount;

        private int _lastChargeDamageAmount;
        private float _currentWindUpTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController) && _type1Primary.GetCurrentChargeAmount() > 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentWindUpTime -= WorldTimeManager.Instance.FixedUpdateTime;
            if (_currentWindUpTime <= 0)
            {
                var spawnPosition = _shootController.GetShootPosition();
                var direction = _shootController.GetShootLookDirection();

                var projectile = Instantiate(_chargedObjectPrefab, spawnPosition, Quaternion.identity);
                var simpleProj = projectile.GetComponent<SimpleProjectile>();
                simpleProj.LaunchProjectile(direction);

                var chargeAmount = _type1Primary.GetCurrentChargeAmount();
                var maxChargeAmount = _type1Primary.GetMaxChargeAmount();
                var mappedDamage = Mathf.CeilToInt(ExtensionFunctions.Map(chargeAmount, 0, maxChargeAmount, _minChargeDamage, _maxChargeDamage));
                var simpleDamage = projectile.GetComponent<SimpleDamageOverrideTrigger>();
                simpleDamage.SetCollisionCallback(HandleProjectileHitCollider);
                simpleDamage.SetDamageAmount(mappedDamage);

                _lastChargeDamageAmount = mappedDamage;
                _playerAnimator.SetInteger(PlayerStaticData.Type_1_Secondary, Random.Range(1, _attackAnimCount + 1));
                _type1Primary.UseStoredCharge(chargeAmount);
                _abilityEnd = true;

                HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
                CustomCameraController.Instance.StartShake(_cameraShaker);
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController)
        {
            _abilityEnd = false;
            _currentWindUpTime = _windUpTime;
        }

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityUpdateDelegate(playerController);

            var chargeAmount = _type1Primary.GetCurrentChargeAmount();
            HUD_PlayerAbilityDisplay.Instance.UpdateCounter(AbilityTrigger.Secondary, $"{chargeAmount}", true);

            var overlayPercent = chargeAmount > 0 ? 0 : 1;
            HUD_PlayerAbilityDisplay.Instance.UpdateOverlay(AbilityTrigger.Secondary, overlayPercent);
        }

        #endregion Unity Functions

        #region External Functions

        private void HandleProjectileHitCollider(Collider other) => _type1Ultimate.AddUltimateCharge(_lastChargeDamageAmount);

        #endregion External Functions
    }
}