#region

using CustomCamera;
using Player.Base;
using Player.Common;
using UI.DisplayManagers.Player;
using UnityEngine;
using World;

#endregion

namespace Player.Type_2
{
    public class Type_2_Ultimate_WaterBubbleFrozenAbility : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _ultimateBurstEffectPrefab;
        [SerializeField] private GameObject _waterBubblePrefab;

        [Header("Components")]
        [SerializeField] private GameObject _parentGameObject;
        [SerializeField] private Animator _playerAnimator;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _cameraShaker;

        [Header("Bubble Data")]
        [SerializeField] private float _abilityCastRadius;
        [SerializeField] private LayerMask _abilityMask;

        [Header("Ultimate Data")]
        [SerializeField] private float _windUpTime;
        [SerializeField] private int _ultimateChargePerSec;
        [SerializeField] private int _maxUltimateChargeAmount;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];

        // Ultimate Data
        private float _ultimateChargeTick;
        private int _currentUltimateAmount;

        private float _currentWindUpTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) =>
            base.AbilityCanStart(playerController) && _currentUltimateAmount >= _maxUltimateChargeAmount;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentWindUpTime -= WorldTimeManager.Instance.FixedUpdateTime;
            if (_currentWindUpTime > 0)
            {
                return;
            }

            var castPosition = transform.position;
            var totalHitColliders = Physics.OverlapSphereNonAlloc(castPosition, _abilityCastRadius, _hitColliders, _abilityMask);
            if (_debugIsActive)
            {
                DebugExtension.DebugWireSphere(transform.position, Color.red, _abilityCastRadius, _debugDisplayDuration);
            }

            for (var i = 0; i < totalHitColliders; i++)
            {
                // Do not target itself
                if (_hitColliders[i].gameObject.GetInstanceID() == _parentGameObject.GetInstanceID())
                {
                    continue;
                }

                if (_hitColliders[i].TryGetComponent(out BasePlayerController targetController))
                {
                    var targetTransform = targetController.transform;
                    var position = targetTransform.position;
                    var waterBubble = Instantiate(_waterBubblePrefab, position, Quaternion.identity, targetTransform);
                    var waterBubbleFrozen = waterBubble.GetComponent<Type_2_Ultimate_WaterBubbleFrozen>();
                    targetController.CheckAndAddExternalAbility(waterBubbleFrozen);
                }
            }

            _currentUltimateAmount = 0;
            _abilityEnd = true;

            _playerAnimator.SetTrigger(PlayerStaticData.Type_2_Ultimate);
            Instantiate(_ultimateBurstEffectPrefab, transform.position, Quaternion.identity);
            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
            CustomCameraController.Instance.StartShake(_cameraShaker);
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController)
        {
            _abilityEnd = false;
            _currentWindUpTime = _windUpTime;
        }

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityUpdateDelegate(playerController);
            DisplayUltimateToHUD();
        }

        public override void UnityFixedUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityFixedUpdateDelegate(playerController);

            if (_currentUltimateAmount < _maxUltimateChargeAmount)
            {
                while (_ultimateChargeTick >= 1)
                {
                    _currentUltimateAmount += _ultimateChargePerSec;
                    _ultimateChargeTick -= 1;
                }

                _ultimateChargeTick += WorldTimeManager.Instance.FixedUpdateTime;
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void AddUltimateCharge(int amount)
        {
            _currentUltimateAmount += amount;
            _currentUltimateAmount = Mathf.Clamp(_currentUltimateAmount, 0, _maxUltimateChargeAmount);
        }

        #endregion External Functions

        #region Utils

        private void DisplayUltimateToHUD()
        {
            var ultimatePercent = (float)_currentUltimateAmount / _maxUltimateChargeAmount * PlayerStaticData.MaxUltimateDisplayLimit;
            HUD_PlayerAbilityDisplay.Instance.UpdateTimer(AbilityTrigger.Ultimate, $"{ultimatePercent:0.0} %", true);

            var overlayPercent = _currentUltimateAmount >= _maxUltimateChargeAmount ? 0 : 1;
            HUD_PlayerAbilityDisplay.Instance.UpdateOverlay(AbilityTrigger.Ultimate, overlayPercent);
            HUD_PlayerAbilityDisplay.Instance.UpdateCounter(AbilityTrigger.Ultimate, $"{_currentUltimateAmount}", _debugIsActive);
        }

        #endregion Utils
    }
}