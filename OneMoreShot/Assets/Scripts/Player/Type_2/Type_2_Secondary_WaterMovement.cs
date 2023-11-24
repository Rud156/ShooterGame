using System;
using System.Collections.Generic;
using Player.Abilities;
using Player.Core;
using Player.Misc;
using UI.Player;
using UnityEngine;

namespace Player.Type_2
{
    public class Type_2_Secondary_WaterMovement : AbilityBase
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _burstDamageMarkerPrefab;
        [SerializeField] private GameObject _markerEffectPrefab;
        [SerializeField] private GameObject _burstDamageEffectPrefab;
        [SerializeField] private GameObject _dashTrailEffectPrefab;
        [SerializeField] private GameObject _holdRunTrailEffectPrefab;

        [Header("Render Data")]
        [SerializeField] private Material _secondaryRenderMaterial;
        [SerializeField] private List<RenderMaterialData> _defaultRenderMaterialData;

        [Header("Hold Type Data")]
        [SerializeField] private Vector3 _holdRunEffectOffset;
        [SerializeField] private float _holdTriggerDuration;
        [SerializeField] private float _holdRunVelocity;
        [SerializeField] private float _holdRunDuration;

        [Header("Tap Type Data")]
        [SerializeField] private Vector3 _dashEffectOffset;
        [SerializeField] private float _dashVelocity;
        [SerializeField] private float _dashDuration;

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
        }

        public override void AbilityUpdate(PlayerController playerController, float deltaTime)
        {
        }

        public override void AbilityEnd(PlayerController playerController)
        {
        }

        #endregion

        #region Ability Conditions

        public override bool AbilityCanStart(PlayerController playerController) => base.AbilityCanStart(playerController) && playerController.IsGrounded;

        public override bool AbilityNeedsToEnd(PlayerController playerController) => _waterMovementState == WaterMovementState.End;

        #endregion

        #region Getters

        public override Vector3 MovementData() => _computedVelocity;

        #endregion

        #region Unity Function Delegates

        public override void UnityStartDelegate(PlayerController playerController)
        {
            base.UnityStartDelegate(playerController);
            _burstDamageMarkers = new List<BurstDamageData>();
        }

        #endregion

        #region Ability State Updates

        private void SetAbilityState(WaterMovementState abilityState) => _waterMovementState = abilityState;

        #endregion

        #region Structs

        private struct BurstDamageData
        {
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

        #endregion

        #region Enums

        private enum WaterMovementState
        {
            Tap,
            Dash,
            Hold,
            End
        }

        #endregion
    }
}