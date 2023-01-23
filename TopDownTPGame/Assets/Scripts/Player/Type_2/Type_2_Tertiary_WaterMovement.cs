#region

using System;
using System.Collections.Generic;
using HealthSystem;
using Player.Base;
using Player.Common;
using UnityEngine;
using UnityEngine.Assertions;

#endregion

namespace Player.Type_2
{
    public class Type_2_Tertiary_WaterMovement : Ability
    {
        private const int MaxCollidersCheck = 10;

        [Header("Prefabs")]
        [SerializeField] private GameObject _markerEffectPrefab;
        [SerializeField] private GameObject _damageBurstEffectPrefab;
        [SerializeField] private GameObject _dashTrailEffectPrefab;
        [SerializeField] private GameObject _holdRunTrailEffectPrefab;

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

        [Header("Damage Data")]
        [SerializeField] private LayerMask _damageCheckMask;
        [SerializeField] private float _damageCheckRadius;
        [SerializeField] private int _damageAmount;

        private GameObject _abilityStateEffectObject;
        private List<BurstDamageData> _burstDamageMarkers;

        private AbilityState _abilityState;
        private float _currentTimer;
        private Vector3 _computedVelocity;

        public override bool AbilityCanStart(BasePlayerController playerController) => playerController.IsGrounded && _currentCooldownDuration <= 0;

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

                case AbilityState.End:
                    // Do nothing here...
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            foreach (var burstDamageData in _burstDamageMarkers)
            {
                burstDamageData.burstDamageMarker.SetDamageAmount(_damageAmount);
                burstDamageData.burstDamageMarker.ApplyDamage();
                Destroy(burstDamageData.markedEffectObject);
                Destroy(burstDamageData.burstDamageMarker);

                var position = burstDamageData.burstDamageMarker.transform.position;
                Instantiate(_damageBurstEffectPrefab, position, Quaternion.identity);
            }

            _burstDamageMarkers.Clear();

            foreach (var playerRenderer in _playerRenderers)
            {
                playerRenderer.material = _defaultMaterial;
            }

            _currentCooldownDuration = _cooldownDuration;

            Destroy(_abilityStateEffectObject);
            _abilityStateEffectObject = null;
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            SetAbilityState(AbilityState.Tap);
            _currentTimer = _holdTriggerDuration;
            _computedVelocity = Vector3.zero;
        }

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            base.UnityStartDelegate(playerController);
            _burstDamageMarkers = new List<BurstDamageData>();
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
            _computedVelocity = forward.normalized * _dashVelocity;
            _computedVelocity.y = 0;

            CheckAndApplyDamageMarker();
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
            var key = playerController.GetTertiaryAbilityKey();
            if (_currentTimer <= 0 || key.KeyPressedThisFrame)
            {
                SetAbilityState(AbilityState.End);
            }

            var forward = transform.forward;

            var movement = forward;
            movement = _holdRunVelocity * movement.normalized;
            _computedVelocity.x = movement.x;
            _computedVelocity.z = movement.z;
            if (!playerController.IsGrounded)
            {
                _computedVelocity.y += Physics.gravity.y * playerController.GravityMultiplier;
            }

            CheckAndApplyDamageMarker();
        }

        private void CheckAndApplyDamageMarker()
        {
            var hitColliders = new Collider[MaxCollidersCheck];
            var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _damageCheckRadius, hitColliders, _damageCheckMask);
            DebugExtension.DebugWireSphere(transform.position, Color.white, _damageCheckRadius);

            for (var i = 0; i < targetsHit; i++)
            {
                // Do not target itself
                if (hitColliders[i] == null || hitColliders[i].gameObject.GetInstanceID() == gameObject.GetInstanceID())
                {
                    continue;
                }

                var hasHealth = hitColliders[i].TryGetComponent(out HealthAndDamage healthAndDamage);
                var hasBurstDamageMarker = hitColliders[i].TryGetComponent(out BurstDamageMarker burstDamageMarker);
                if (hasHealth && hasBurstDamageMarker)
                {
                    var ownerId = burstDamageMarker.GetOwner();
                    var myId = GetInstanceID();
                    if (ownerId != myId)
                    {
                        hasBurstDamageMarker = false;
                    }
                }

                if (hasHealth && !hasBurstDamageMarker)
                {
                    var burstDamage = hitColliders[i].gameObject.AddComponent<BurstDamageMarker>();
                    burstDamage.SetOwner(GetInstanceID());

                    var targetTransform = hitColliders[i].transform;
                    var effect = Instantiate(_markerEffectPrefab, targetTransform.position, Quaternion.identity, targetTransform);

                    _burstDamageMarkers.Add(new BurstDamageData()
                    {
                        burstDamageMarker = burstDamage,
                        markedEffectObject = effect,
                    });
                }
            }
        }

        private void SetAbilityState(AbilityState abilityState) => _abilityState = abilityState;

        #endregion Utils

        #region Structs

        private struct BurstDamageData
        {
            public BurstDamageMarker burstDamageMarker;
            public GameObject markedEffectObject;
        }

        #endregion Structs

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