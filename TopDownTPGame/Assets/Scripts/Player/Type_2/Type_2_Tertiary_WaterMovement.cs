using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Input;

namespace Player.Type_2
{
    public class Type_2_Tertiary_WaterMovement : Ability
    {
        [Header("Components")]
        [SerializeField] private GameObject _waterMovementPrefab;

        [Header("Water Data")]
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _abilityDuration;

        private float _currentDuration;
        private Vector3 _computedVelocity;

        // This is added since the Ability Update is called in the same frame that the Ability is Started
        // So to handle the key a second time a delay needed to be added for 1 frame
        private bool _abilityUpdatedOnce;

        public override bool AbilityCanStart(BasePlayerController playerController) => playerController.IsGrounded;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _currentDuration <= 0;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            Vector3 coreInput = playerController.GetCoreMoveInput();
            Vector3 movement = forward * coreInput.y + right * coreInput.x;
            movement = _movementSpeed * movement.normalized;
            movement.y = 0;

            _computedVelocity = movement;
            _currentDuration -= Time.fixedDeltaTime;

            PlayerInputKey key = playerController.GetTertiaryAbilityKey();
            if (key.keyPressedThisFrame && _abilityUpdatedOnce)
            {
                _currentDuration = 0;
            }

            _abilityUpdatedOnce = true;
        }

        public override void EndAbility(BasePlayerController playerController)
        {
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            _currentDuration = _abilityDuration;
            _abilityUpdatedOnce = false;
        }

        public override Vector3 GetMovementData() => _computedVelocity;
    }
}