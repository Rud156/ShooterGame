#region

using System;
using System.Collections.Generic;
using Player.Base;
using Player.Common;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

#endregion

namespace Player.Type_2
{
    public class Type_2_Tertiary_WaterMovement : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _burstTriggerPrefab;
        [SerializeField] private GameObject _dashTrailEffectPrefab;
        [SerializeField] private GameObject _holdRunTrailEffectPrefab;

        [Header("Components")]
        [SerializeField] private CharacterController _characterController;

        [Header("Render Data")]
        [SerializeField] private List<Renderer> _playerRenderers;
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private Material _abilityMaterial;

        [Header("Hold Type Data")]
        [SerializeField] private Vector3 _holdRunEffectOffset;
        [SerializeField] private float _holdTriggerDuration;
        [SerializeField] private float _holdRunVelocity;
        [SerializeField] private float _holdRunDuration;

        [Header("Tap Type Data")]
        [SerializeField] private Vector3 _dashEffectOffset;
        [SerializeField] private float _dashVelocity;
        [SerializeField] private float _dashDuration;

        private GameObject _abilityStateEffectObject;

        private AbilityState _abilityState;
        private float _currentTimer;
        private Vector3 _computedVelocity;

        public override bool AbilityCanStart(BasePlayerController playerController) => playerController.IsGrounded;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityState == AbilityState.End;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            UpdateTriggerTypeSelect(playerController);
            switch (_abilityState)
            {
                case AbilityState.Tap:
                    // Do nothing here...
                    break;

                case AbilityState.Dash:
                    UpdateDashMovement();
                    break;

                case AbilityState.HoldRun:
                    UpdateHoldRunMovement(playerController);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            foreach (var playerRenderer in _playerRenderers)
            {
                playerRenderer.material = _defaultMaterial;
            }

            Destroy(_abilityStateEffectObject);
            _abilityStateEffectObject = null;
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            SetAbilityState(AbilityState.Tap);
            _currentTimer = _holdTriggerDuration;
        }

        public override Vector3 GetMovementData() => _computedVelocity;

        #region Utils

        private void UpdateTriggerTypeSelect(BasePlayerController playerController)
        {
            _currentTimer -= Time.fixedDeltaTime;

            var key = playerController.GetTertiaryAbilityKey();
            switch (_abilityState)
            {
                case AbilityState.Tap when key.KeyReleasedThisFrame:
                {
                    _currentTimer = _dashDuration;
                    SetupDashMovement();
                    SetAbilityState(AbilityState.Dash);
                }
                    break;

                case AbilityState.Tap when _currentTimer <= 0:
                {
                    _currentTimer = _holdRunDuration;
                    SetupHoldRunMovement();
                    SetAbilityState(AbilityState.HoldRun);
                }
                    break;
            }
        }

        private void SetupDashMovement()
        {
            foreach (var playerRenderer in _playerRenderers)
            {
                playerRenderer.material = _abilityMaterial;
            }

            Assert.IsNull(_abilityStateEffectObject, "Effect should be NULL here");

            var playerTransform = transform;
            _abilityStateEffectObject = Instantiate(_holdRunTrailEffectPrefab, playerTransform.position, Quaternion.identity, playerTransform);
            _abilityStateEffectObject.transform.localPosition = _dashEffectOffset;
        }

        private void UpdateDashMovement()
        {
            _currentTimer -= Time.fixedDeltaTime;
            if (_currentTimer <= 0)
            {
                SetAbilityState(AbilityState.End);
            }

            var characterTransform = transform;
            var forward = characterTransform.forward;

            // Override X and Z
            _computedVelocity = forward;
            _computedVelocity = _dashVelocity * _computedVelocity.normalized;
            _computedVelocity.y = 0;
        }

        private void SetupHoldRunMovement()
        {
            foreach (var playerRenderer in _playerRenderers)
            {
                playerRenderer.material = _abilityMaterial;
            }

            Assert.IsNull(_abilityStateEffectObject, "Effect should be NULL here");

            var playerTransform = transform;
            _abilityStateEffectObject = Instantiate(_dashTrailEffectPrefab, playerTransform.position, Quaternion.identity, playerTransform);
            _abilityStateEffectObject.transform.localPosition = _holdRunEffectOffset;
            _computedVelocity = Vector3.zero;
        }

        private void UpdateHoldRunMovement(BasePlayerController playerController)
        {
            _currentTimer -= Time.fixedDeltaTime;

            var coreInput = playerController.GetCoreMoveInput();
            var playerTransform = transform;
            var forward = playerTransform.forward;
            var right = playerTransform.right;

            var movement = forward * coreInput.y + right * coreInput.x;
            movement = _holdRunVelocity * movement.normalized;
            _computedVelocity.x = movement.x;
            _computedVelocity.z = movement.z;
            if (!playerController.IsGrounded)
            {
                _computedVelocity.y += Physics.gravity.y * playerController.GravityMultiplier;
            }

            // TODO: Complete Trigger on another Health Object...
        }

        private void SetAbilityState(AbilityState abilityState) => _abilityState = abilityState;

        #endregion Utils

        #region Enums

        private enum AbilityState
        {
            Tap,
            Dash,
            HoldRun,
            End,
        }

        #endregion Enums
    }
}