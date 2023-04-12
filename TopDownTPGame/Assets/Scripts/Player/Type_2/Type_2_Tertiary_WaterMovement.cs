#region

using System;
using System.Collections.Generic;
using HealthSystem;
using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;
using UnityEngine.Assertions;
using Utils.Common;

#endregion

namespace Player.Type_2
{
    public class Type_2_Tertiary_WaterMovement : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _markerEffectPrefab;
        [SerializeField] private GameObject _burstDamageEffectPrefab;
        [SerializeField] private GameObject _dashTrailEffectPrefab;
        [SerializeField] private GameObject _holdRunTrailEffectPrefab;

        [Header("Components")]
        [SerializeField] private Animator _playerAnimator;

        [Header("Render Data")]
        [SerializeField] private Material _tertiaryMaterial;
        [SerializeField] private List<TertiaryMaterialData> _tertiaryMaterialDatas;

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

        private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];

        private GameObject _abilityStateEffectObject;
        private List<BurstDamageData> _burstDamageMarkers;

        private AbilityState _abilityState;
        private float _currentTimer;
        private Vector3 _computedVelocity;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) =>
            base.AbilityCanStart(playerController) && playerController.IsGrounded && _currentCooldownDuration <= 0;

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
            // Clear all the Burst Damage Markers
            foreach (var burstDamageData in _burstDamageMarkers)
            {
                burstDamageData.BurstDamageMarker.SetDamageAmount(_damageAmount);
                burstDamageData.BurstDamageMarker.ApplyDamage();
                Destroy(burstDamageData.MarkedEffectObject);
                Destroy(burstDamageData.BurstDamageMarker);

                var position = burstDamageData.BurstDamageMarker.transform.position;
                Instantiate(_burstDamageEffectPrefab, position, Quaternion.identity);
            }

            _burstDamageMarkers.Clear();

            // Reset all the materials
            ClearTertiaryMaterials();

            Destroy(_abilityStateEffectObject);
            _abilityStateEffectObject = null;
            _currentCooldownDuration = _cooldownDuration;
            _playerAnimator.SetBool(PlayerStaticData.Type_2_Tertiary, false);
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            SetAbilityState(AbilityState.Tap);
            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);

            _currentTimer = _holdTriggerDuration;
            _computedVelocity = Vector3.zero;
        }

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            base.UnityStartDelegate(playerController);
            _burstDamageMarkers = new List<BurstDamageData>();
        }

        #endregion Unity Functions

        #region Specific Data

        public override Vector3 GetMovementData() => _computedVelocity;

        #endregion Specific Data

        #region Utils

        #region State Updates

        private void UpdateTriggerTypeSelect(BasePlayerController playerController)
        {
            _currentTimer -= GlobalStaticData.FixedUpdateTime;

            var key = playerController.GetKeyForAbilityTrigger(_abilityTrigger);
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
            Assert.IsNull(_abilityStateEffectObject, "Effect should be NULL here");
            ApplyTertiaryMaterials();

            var playerTransform = transform;
            _abilityStateEffectObject = Instantiate(_holdRunTrailEffectPrefab, playerTransform.position, Quaternion.identity, playerTransform);
            _abilityStateEffectObject.transform.localPosition = _dashEffectOffset;
            _playerAnimator.SetBool(PlayerStaticData.Type_2_Tertiary, true);
        }

        private void UpdateDashMovement()
        {
            _currentTimer -= GlobalStaticData.FixedUpdateTime;
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
            Assert.IsNull(_abilityStateEffectObject, "Effect should be NULL here");
            ApplyTertiaryMaterials();

            var playerTransform = transform;
            _abilityStateEffectObject = Instantiate(_dashTrailEffectPrefab, playerTransform.position, Quaternion.identity, playerTransform);
            _abilityStateEffectObject.transform.localPosition = _holdRunEffectOffset;
            _computedVelocity = Vector3.zero;
            _playerAnimator.SetBool(PlayerStaticData.Type_2_Tertiary, true);
        }

        private void UpdateHoldRunMovement(BasePlayerController playerController)
        {
            _currentTimer -= GlobalStaticData.FixedUpdateTime;
            var key = playerController.GetKeyForAbilityTrigger(_abilityTrigger);
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
            var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _damageCheckRadius, _hitColliders, _damageCheckMask);

            for (var i = 0; i < targetsHit; i++)
            {
                // Do not target itself
                if (_hitColliders[i] == null || _hitColliders[i].gameObject.GetInstanceID() == gameObject.GetInstanceID())
                {
                    continue;
                }

                var hasHealth = _hitColliders[i].TryGetComponent(out HealthAndDamage healthAndDamage);
                var hasBurstDamageMarker = _hitColliders[i].TryGetComponent(out BurstDamageMarker burstDamageMarker);
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
                    var burstDamage = _hitColliders[i].gameObject.AddComponent<BurstDamageMarker>();
                    burstDamage.SetOwner(GetInstanceID());

                    var targetTransform = _hitColliders[i].transform;
                    var effect = Instantiate(_markerEffectPrefab, targetTransform.position, Quaternion.identity, targetTransform);

                    _burstDamageMarkers.Add(new BurstDamageData()
                    {
                        BurstDamageMarker = burstDamage,
                        MarkedEffectObject = effect,
                    });
                }
            }
        }

        #endregion State Updates

        #region Material Utils

        private void ApplyTertiaryMaterials()
        {
            foreach (var data in _tertiaryMaterialDatas)
            {
                var materials = new List<Material>();
                for (var j = 0; j < data.materialCount; j++)
                {
                    materials.Add(_tertiaryMaterial);
                }

                data.renderer.materials = materials.ToArray();
            }
        }

        private void ClearTertiaryMaterials()
        {
            foreach (var data in _tertiaryMaterialDatas)
            {
                var materials = new List<Material>();
                for (var j = 0; j < data.materialCount; j++)
                {
                    materials.Add(data.materialList[j]);
                }

                data.renderer.materials = materials.ToArray();
            }
        }

        #endregion Material Utils

        private void SetAbilityState(AbilityState abilityState) => _abilityState = abilityState;

        #endregion Utils

        #region Structs

        private struct BurstDamageData
        {
            public BurstDamageMarker BurstDamageMarker;
            public GameObject MarkedEffectObject;
        }

        [Serializable]
        private struct TertiaryMaterialData
        {
            public Renderer renderer;
            public int materialCount;
            public List<Material> materialList;
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