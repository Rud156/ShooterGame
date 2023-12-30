using HeallthSystem;
using HealthSystem.DamageSystems;
using NUnit.Framework;
using Player.Abilities;
using Player.Core;
using Player.Misc;
using System;
using System.Collections.Generic;
using UI.Player;
using UnityEngine;
using Utils.Common;

namespace Player.Type_2
{
    public class Type_2_Secondary_WaterMovement : AbilityBase
    {
        private static readonly int Type_2_SecondaryAnimParam = Animator.StringToHash("Type_2_Secondary");

        [Header("Prefabs")]
        [SerializeField] private GameObject _burstDamageMarkerPrefab;
        [SerializeField] private GameObject _burstMarkerEffectPrefab;
        [SerializeField] private GameObject _burstDamageEffectPrefab;
        [SerializeField] private GameObject _dashTrailEffectPrefab;
        [SerializeField] private GameObject _holdRunTrailEffectPrefab;

        [Header("Render Data")]
        [SerializeField] private Material _secondaryRenderMaterial;
        [SerializeField] private List<RenderMaterialData> _defaultRenderMaterialData;

        [Header("Tap Type Data")]
        [SerializeField] private Vector3 _dashEffectOffset;
        [SerializeField] private float _dashVelocity;
        [SerializeField] private float _dashDuration;

        [Header("Hold Type Data")]
        [SerializeField] private Vector3 _holdRunEffectOffset;
        [SerializeField] private float _holdTriggerDuration;
        [SerializeField] private float _holdRunVelocity;
        [SerializeField] private float _holdRunDuration;

        [Header("Damage Data")]
        [SerializeField] private LayerMask _damageCheckMask;
        [SerializeField] private float _damageCheckRadius;
        [SerializeField] private int _damageAmount;

        private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];

        private GameObject _waterMovementEffectInstance;
        private List<BurstDamageData> _burstDamageMarkers;

        private WaterMovementState _waterMovementState;
        private float _currentTimer;
        private Vector3 _computedVelocity;

        #region Core Ability Functions

        public override void AbilityStart(PlayerController playerController)
        {
            base.AbilityStart(playerController);
            SetAbilityState(WaterMovementState.Tap);
            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);

            _currentTimer = _holdTriggerDuration;
            _computedVelocity = Vector3.zero;
        }

        public override void AbilityFixedUpdate(PlayerController playerController, float fixedDeltaTime)
        {
            switch (_waterMovementState)
            {
                case WaterMovementState.Tap:
                case WaterMovementState.End:
                    // Do nothing here...
                    break;

                case WaterMovementState.Dash:
                    UpdateDashMovement(fixedDeltaTime);
                    break;

                case WaterMovementState.HoldRun:
                    UpdateHoldRunMovement(playerController, fixedDeltaTime);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void AbilityUpdate(PlayerController playerController, float deltaTime) => UpdateAbilityStateTypeSelector(playerController);

        public override void AbilityEnd(PlayerController playerController)
        {
            // Calculate Burst Damage
            foreach (var burstDamageData in _burstDamageMarkers)
            {
                if (burstDamageData.BurstDamageObject == null)
                {
                    continue;
                }

                burstDamageData.BurstDamageMarker.SetDamageAmount(_damageAmount);
                burstDamageData.BurstDamageMarker.ApplyDamage();

                var position = burstDamageData.BurstDamageMarker.transform.position;
                Instantiate(_burstDamageEffectPrefab, position, Quaternion.identity);

                Destroy(burstDamageData.MarkedEffectObject);
                Destroy(burstDamageData.BurstDamageObject);
                Destroy(burstDamageData.BurstDamageMarker);
            }

            _burstDamageMarkers.Clear();
            ClearAbilityMaterials();

            Destroy(_waterMovementEffectInstance);
            _waterMovementEffectInstance = null;
            _currentCooldownDuration = _abilityCooldownDuration;
            _playerAnimator.SetBool(Type_2_SecondaryAnimParam, false);
        }

        #endregion Core Ability Functions

        #region Ability Conditions

        public override bool AbilityCanStart(PlayerController playerController, bool ignoreCooldown = false) => base.AbilityCanStart(playerController) && playerController.IsGrounded;

        public override bool AbilityNeedsToEnd(PlayerController playerController) => _waterMovementState == WaterMovementState.End;

        #endregion Ability Conditions

        #region Getters

        public override Vector3 GetMovementData() => _computedVelocity;

        #endregion Getters

        #region Unity Function Delegates

        public override void UnityStartDelegate(PlayerController playerController)
        {
            base.UnityStartDelegate(playerController);
            _burstDamageMarkers = new List<BurstDamageData>();
        }

        #endregion Unity Function Delegates

        #region Ability State Updates

        private void UpdateAbilityStateTypeSelector(PlayerController playerController)
        {
            _currentTimer -= Time.deltaTime;

            var key = playerController.GetKeyForAbilityTrigger(_abilityTrigger);
            switch (_waterMovementState)
            {
                case WaterMovementState.Tap when !key.KeyPressed:
                    {
                        _currentTimer = _dashDuration;
                        SetupDashMovement();
                        SetAbilityState(WaterMovementState.Dash);
                    }
                    break;

                case WaterMovementState.Tap when _currentTimer <= 0:
                    {
                        _currentTimer = _holdRunDuration;
                        SetupHoldRunMovement();
                        SetAbilityState(WaterMovementState.HoldRun);
                    }
                    break;
            }
        }

        #region Dash Movement

        private void SetupDashMovement()
        {
            Assert.IsNull(_waterMovementEffectInstance, "Effect should be NULL here");
            ApplyAbilityMaterials();

            var playerTransform = transform;
            _waterMovementEffectInstance = Instantiate(_holdRunTrailEffectPrefab, playerTransform.position, Quaternion.identity, playerTransform);
            _waterMovementEffectInstance.transform.localPosition = _dashEffectOffset;
            _playerAnimator.SetBool(Type_2_SecondaryAnimParam, true);
        }

        private void UpdateDashMovement(float fixedDeltaTime)
        {
            _currentTimer -= fixedDeltaTime;
            if (_currentTimer <= 0)
            {
                SetAbilityState(WaterMovementState.End);
            }

            var characterTransform = transform;
            var forward = characterTransform.forward;

            // Override X and Z
            _computedVelocity = forward.normalized * _dashVelocity;
            _computedVelocity.y = 0;

            CheckAndApplyDamageMarker();
        }

        #endregion Dash Movement

        #region Hold Movement

        private void SetupHoldRunMovement()
        {
            Assert.IsNull(_waterMovementEffectInstance, "Effect should be NULL here");
            ApplyAbilityMaterials();

            var playerTransform = transform;
            _waterMovementEffectInstance = Instantiate(_dashTrailEffectPrefab, playerTransform.position, Quaternion.identity, playerTransform);
            _waterMovementEffectInstance.transform.localPosition = _holdRunEffectOffset;
            _computedVelocity = Vector3.zero;
            _playerAnimator.SetBool(Type_2_SecondaryAnimParam, true);
        }

        private void UpdateHoldRunMovement(PlayerController playerController, float fixedDeltaTime)
        {
            _currentTimer -= fixedDeltaTime;
            var key = playerController.GetKeyForAbilityTrigger(_abilityTrigger);
            if (_currentTimer <= 0 || key.KeyPressedThisFrame)
            {
                SetAbilityState(WaterMovementState.End);
            }

            var forward = transform.forward;
            var coreInput = playerController.CoreMovementInput;

            var movement = forward;
            movement = _holdRunVelocity * movement.normalized;
            _computedVelocity.x = movement.x;
            _computedVelocity.z = movement.z;
            if (!playerController.IsGrounded)
            {
                _computedVelocity.y += Physics.gravity.y * playerController.GravityMultiplier;
            }

            playerController.ForcePlayerRotation();
            CheckAndApplyDamageMarker();
        }

        #endregion Hold Movement

        private void SetAbilityState(WaterMovementState abilityState) => _waterMovementState = abilityState;

        private void CheckAndApplyDamageMarker()
        {
            var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _damageCheckRadius, _hitColliders, _damageCheckMask);

            for (var i = 0; i < targetsHit; i++)
            {
                var hitOwnerData = _hitColliders[i].GetComponent<OwnerData>();
                if (hitOwnerData == null || hitOwnerData.OwnerId == _ownerData.OwnerId)
                {
                    continue;
                }

                var hasHealth = _hitColliders[i].TryGetComponent(out HealthAndDamage _);
                var targetBurstDamageMarker = _hitColliders[i].GetComponentInChildren<BurstDamageMarker>();
                var hasBurstDamageMarker = targetBurstDamageMarker != null;
                if (hasHealth && hasBurstDamageMarker)
                {
                    var targetOwnerId = targetBurstDamageMarker.GetComponent<OwnerData>().OwnerId;
                    if (targetOwnerId != _ownerData.OwnerId)
                    {
                        hasBurstDamageMarker = false;
                    }
                }

                if (hasHealth && !hasBurstDamageMarker)
                {
                    var burstDamage = Instantiate(_burstDamageMarkerPrefab, _hitColliders[i].transform.position, Quaternion.identity, _hitColliders[i].transform);
                    var ownerData = burstDamage.GetComponent<OwnerData>();
                    ownerData.OwnerId = _ownerData.OwnerId;

                    var targetTransform = _hitColliders[i].transform;
                    var effect = Instantiate(_burstMarkerEffectPrefab, targetTransform.position, Quaternion.identity, targetTransform);

                    _burstDamageMarkers.Add(new BurstDamageData()
                    {
                        BurstDamageMarker = burstDamage.GetComponent<BurstDamageMarker>(),
                        MarkedEffectObject = effect,
                        BurstDamageObject = burstDamage
                    });
                }
            }
        }

        #endregion Ability State Updates

        #region Material Updates

        private void ApplyAbilityMaterials()
        {
            var materials = new List<Material>();
            foreach (var modelData in _defaultRenderMaterialData)
            {
                for (var i = 0; i < modelData.materialCount; i++)
                {
                    materials.Add(_secondaryRenderMaterial);
                }

                modelData.renderer.materials = materials.ToArray();
                materials.Clear();
            }
        }

        private void ClearAbilityMaterials()
        {
            var materials = new List<Material>();
            foreach (var modelData in _defaultRenderMaterialData)
            {
                for (var i = 0; i < modelData.materialCount; i++)
                {
                    materials.Add(modelData.materialList[i]);
                }

                modelData.renderer.materials = materials.ToArray();
                materials.Clear();
            }
        }

        #endregion Material Updates

        #region Structs

        private struct BurstDamageData
        {
            public BurstDamageMarker BurstDamageMarker;
            public GameObject MarkedEffectObject;
            public GameObject BurstDamageObject;
        }

        [Serializable]
        private struct RenderMaterialData
        {
            public Renderer renderer;
            public int materialCount;
            public List<Material> materialList;
        }

        #endregion Structs

        #region Enums

        private enum WaterMovementState
        {
            Tap,
            Dash,
            HoldRun,
            End
        }

        #endregion Enums
    }
}