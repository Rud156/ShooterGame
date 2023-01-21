#region

using System.Collections.Generic;
using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_2
{
    public class Type_2_Tertiary_WaterMovement : Ability
    {
        [Header("Components")]
        [SerializeField] private CharacterController _characterController;

        [Header("Render Data")]
        [SerializeField] private List<Renderer> _playerRenderers;
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private Material _abilityMaterial;

        [Header("Hold Type Data")]
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _abilityDuration;

        [Header("Tap Type Data")]
        [SerializeField] private float _dashVelocity;
        [SerializeField] private float _dashDuration;

        private float _currentDuration;
        private Vector3 _computedVelocity;

        public override bool AbilityCanStart(BasePlayerController playerController) => playerController.IsGrounded;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _currentDuration <= 0;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
        }

        public override void EndAbility(BasePlayerController playerController)
        {
        }

        public override void StartAbility(BasePlayerController playerController)
        {
        }

        public override Vector3 GetMovementData() => _computedVelocity;
    }
}