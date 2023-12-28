using Player.Abilities;
using Player.Core;
using UnityEngine;

namespace Assets.Scripts.Player.CommonAbilities
{
    public class Ability_StunDurationBased : AbilityBase
    {
        [Header("Stun Data")]
        [SerializeField] private float _stunDuration;

        private float _currentStunDuration;

        #region Core Ability Functions

        public override void AbilityStart(PlayerController playerController)
        {
            base.AbilityStart(playerController);
            _currentStunDuration = _stunDuration;
        }

        public override void AbilityFixedUpdate(PlayerController playerController, float fixedDeltaTime) => _currentStunDuration -= fixedDeltaTime;

        public override void AbilityUpdate(PlayerController playerController, float deltaTime)
        {
        }

        public override void AbilityEnd(PlayerController playerController) => _currentStunDuration = 0;

        #endregion Core Ability Functions

        #region Ability Conditions

        public override bool AbilityCanStart(PlayerController playerController, bool ignoreCooldown = false) => true;

        public override bool AbilityNeedsToEnd(PlayerController playerController) => _currentStunDuration <= 0;

        #endregion Ability Conditions

        #region Getters

        public override Vector3 GetMovementData()
        {
            var playerVelocity = _playerController.CharacterVelocity;
            return new Vector3(0, playerVelocity.y, 0);
        }

        #endregion Getters
    }
}