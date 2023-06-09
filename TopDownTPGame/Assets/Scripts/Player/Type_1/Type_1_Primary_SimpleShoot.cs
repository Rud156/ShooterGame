#region

using Ability_Scripts.Projectiles;
using CustomCamera;
using HealthSystem;
using Player.Base;
using Player.Common;
using UI.DisplayManagers.Player;
using UnityEngine;
using Utils.Common;
using Utils.Misc;
using World;

#endregion

namespace Player.Type_1
{
    public class Type_1_Primary_SimpleShoot : Ability
    {
        private const int MaxChargeAmount = 100;

        [Header("Prefabs")]
        [SerializeField] private GameObject _projectilePrefab;

        [Header("Components")]
        [SerializeField] private GameObject _parent;
        [SerializeField] private PlayerBaseShootController _shootController;
        [SerializeField] private Type_1_Ultimate_WarCryPulseAbility _type1Ultimate;
        [SerializeField] private Animator _playerAnimator;

        [Header("Simple Shoot Data")]
        [SerializeField] private float _fireRate;
        [SerializeField] private float _overheatTime;
        [SerializeField] private float _overheatCooldownMultiplier;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _cameraShaker;

        [Header("Charge Data")]
        [SerializeField] private float _chargeGainedForShootStaticObject;
        [SerializeField] private float _chargeGainedForShootCharacters;
        [SerializeField] private float _chargeDecayDelay;
        [SerializeField] private float _chargeDecayRate;

        [Header("Ultimate Charge Data")]
        [SerializeField] private int _ultimateChargeAmount;

        [Header("Animations")]
        [SerializeField] private int _attackAnimCount;

        private float _nextShootTime;
        private bool _abilityEnd;

        private float _currentOverheatTime;

        private float _currentChargeAmount;
        private float _currentChargeDecay;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (Time.time >= _nextShootTime)
            {
                _nextShootTime = Time.time + _fireRate;
                var spawnPosition = _shootController.GetShootPosition();
                var direction = _shootController.GetShootLookDirection();

                var projectile = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);
                var simpleProj = projectile.GetComponent<SimpleProjectile>();
                var simpleDamageTrigger = projectile.GetComponent<SimpleDamageTrigger>();
                var ownerData = projectile.GetComponent<OwnerData>();

                simpleProj.LaunchProjectile(direction);
                simpleDamageTrigger.SetParent(_parent);
                ownerData.OwnerId = _parent.GetInstanceID();

                var simpleDamage = projectile.GetComponent<SimpleDamageTrigger>();
                simpleDamage.SetCollisionCallback(HandleProjectileHitCollider);

                _playerAnimator.SetInteger(PlayerStaticData.Type_1_Primary, Random.Range(1, _attackAnimCount + 1));
                HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
                CustomCameraController.Instance.StartShake(_cameraShaker);

                _currentOverheatTime += _fireRate;
            }

            if (_currentOverheatTime >= _overheatTime)
            {
                _currentCooldownDuration = _cooldownDuration;
                _currentOverheatTime = 0;
                _abilityEnd = true;
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

        #region Unity Functions

        public override void UnityFixedUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityFixedUpdateDelegate(playerController);

            if (_currentOverheatTime > 0 && _abilityEnd)
            {
                _currentOverheatTime -= WorldTimeManager.Instance.FixedUpdateTime * _overheatCooldownMultiplier;
            }

            if (_currentChargeDecay > 0)
            {
                _currentChargeDecay -= WorldTimeManager.Instance.FixedUpdateTime;
            }
            else
            {
                if (_currentChargeAmount > 0)
                {
                    _currentChargeAmount -= WorldTimeManager.Instance.FixedUpdateTime * _chargeDecayRate;
                }
            }

            // This is a fallback since adding/subtracting is not checking limits
            _currentChargeAmount = Mathf.Clamp(_currentChargeAmount, 0, MaxChargeAmount);
        }

        #endregion Unity Functions

        #region External Functions

        private void HandleProjectileHitCollider(Collider other)
        {
            if (other.CompareTag(TagManager.Player))
            {
                _currentChargeAmount += _chargeGainedForShootCharacters;
                _currentChargeDecay = _chargeDecayDelay;
            }
            else if (other.TryGetComponent(out HealthAndDamage _))
            {
                _currentChargeAmount += _chargeGainedForShootStaticObject;
                _currentChargeDecay = _chargeDecayDelay;
            }

            _type1Ultimate.AddUltimateCharge(_ultimateChargeAmount);
        }

        public int GetCurrentChargeAmount() => Mathf.CeilToInt(_currentChargeAmount);

        public void UseStoredCharge(int amount) => _currentChargeAmount -= amount;

        public int GetMaxChargeAmount() => MaxChargeAmount;

        #endregion External Functions
    }
}