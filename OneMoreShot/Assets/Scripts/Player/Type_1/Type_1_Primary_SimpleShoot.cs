using CustomCamera;
using Player.Abilities;
using Player.Core;
using UI.Player;
using UnityEngine;
using World;

namespace Player.Type_1
{
    public class Type_1_Primary_SimpleShoot : AbilityBase
    {
        private static readonly int Type_1_PrimaryAnimParam = Animator.StringToHash("Type_1_Primary");

        [Header("Prefabs")]
        [SerializeField] private GameObject _abilityProjectile;

        [Header("Shoot Data")]
        [SerializeField] private float _fireRate;
        [SerializeField] private float _overheatDurationAmount;
        [SerializeField] private float _overheatCooldownMultiplier;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _abilityCameraShaker;

        private float _nextShootDuration;
        private bool _abilityMarkedForEnd;

        private float _currentOverheatTime;

        #region Core Ability Functions

        public override void AbilityStart(PlayerController playerController)
        {
            base.AbilityStart(playerController);
            _abilityMarkedForEnd = false;
        }

        public override void AbilityFixedUpdate(PlayerController playerController, float fixedDeltaTime)
        {
            if (_nextShootDuration <= 0)
            {
                _nextShootDuration = _fireRate;
                _currentOverheatTime += _fireRate;

                var spawnPosition = _playerShootController.GetShootPosition();
                var direction = _playerShootController.GetShootLookDirection();

                var projectile = Instantiate(_abilityProjectile, spawnPosition, Quaternion.identity);
                // TODO: Launch The Projectile

                _playerAnimator.SetTrigger(Type_1_PrimaryAnimParam);
                HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
                CustomCameraController.Instance.StartShake(_abilityCameraShaker);
            }
            else
            {
                _nextShootDuration -= fixedDeltaTime;
            }

            if (_currentOverheatTime >= _overheatDurationAmount)
            {
                _currentCooldownDuration = _abilityCooldownDuration;
                _currentOverheatTime = 0;
                _abilityMarkedForEnd = true;
            }
        }

        public override void AbilityUpdate(PlayerController playerController, float deltaTime)
        {
            var inputKey = _playerController.GetKeyForAbilityTrigger(_abilityTrigger);
            if (inputKey.KeyReleasedThisFrame || !inputKey.KeyPressed)
            {
                _abilityMarkedForEnd = true;
            }
        }

        public override void AbilityEnd(PlayerController playerController) => _abilityMarkedForEnd = true;

        #endregion

        #region Ability Conditions

        public override bool AbilityCanStart(PlayerController playerController) => base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(PlayerController playerController) => _abilityMarkedForEnd;

        #endregion

        #region Unity Function Delegates

        public override void UnityFixedUpdateDelegate(PlayerController playerController)
        {
            base.UnityFixedUpdateDelegate(playerController);
            if (_currentOverheatTime > 0)
            {
                _currentOverheatTime -= WorldTimeManager.Instance.FixedUpdateTime * _overheatCooldownMultiplier;
            }
        }

        #endregion
    }
}