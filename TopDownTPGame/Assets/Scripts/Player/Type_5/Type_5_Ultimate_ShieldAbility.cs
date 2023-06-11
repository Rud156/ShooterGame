#region

using CustomCamera;
using Player.Base;
using Player.Common;
using Player.Type_5;
using UI.DisplayManagers.Player;
using UnityEngine;
using Utils.Common;
using World;

#endregion

public class Type_5_Ultimate_ShieldAbility : Ability
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _shieldPrefab;
    [SerializeField] private GameObject _shieldBurstEffectPrefab;

    [Header("Components")]
    [SerializeField] private Animator _playerAnimator;

    [Header("Shield Data")]
    [SerializeField] private float _shieldDeployRadius;
    [SerializeField] private LayerMask _shieldDeployMask;

    [Header("Ultimate Data")]
    [SerializeField] private float _windUptime;
    [SerializeField] private int _ultimateChargePerSec;
    [SerializeField] private int _maxUltimateChargeAmount;

    [Header("Camera Data")]
    [SerializeField] private CameraShaker _cameraShaker;

    [Header("Debug")]
    [SerializeField] private bool _debugIsActive;

    private float _currentWindUpTime;
    private bool _abilityEnd;

    private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];

    // Ultimate Data
    private float _ultimateChargeTick;
    private int _currentUltimateAmount;

    #region Ability Functions

    public override bool AbilityCanStart(BasePlayerController playerController) =>
        base.AbilityCanStart(playerController) && _currentUltimateAmount >= _maxUltimateChargeAmount;

    public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

    public override void AbilityUpdate(BasePlayerController playerController)
    {
        _currentWindUpTime -= WorldTimeManager.Instance.FixedUpdateTime;
        if (_currentWindUpTime <= 0)
        {
            var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _shieldDeployRadius, _hitColliders, _shieldDeployMask);
            for (var i = 0; i < targetsHit; i++)
            {
                if (_hitColliders[i].TryGetComponent(out BasePlayerController targetController))
                {
                    var targetTransform = _hitColliders[i].transform;
                    var shield = Instantiate(_shieldPrefab, targetTransform.position, Quaternion.identity, targetTransform);
                    var targetOwnerData = targetTransform.GetComponent<OwnerData>();
                    var shieldAbility = shield.GetComponent<Type_5_Ultimate_EngineerShield>();
                    var shieldOwnerData = shield.GetComponent<OwnerData>();

                    shieldOwnerData.OwnerId = targetOwnerData.OwnerId;
                    targetController.CheckAndAddExternalAbility(shieldAbility);
                }
            }

            _abilityEnd = true;

            Instantiate(_shieldBurstEffectPrefab, transform.position, Quaternion.identity);
            CustomCameraController.Instance.StartShake(_cameraShaker);
        }
    }

    public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

    public override void StartAbility(BasePlayerController playerController)
    {
        _currentWindUpTime = _windUptime;
        _currentUltimateAmount = 0;
        _abilityEnd = false;
        _playerAnimator.SetTrigger(PlayerStaticData.Type_5_Ultimate);

        CustomCameraController.Instance.StartShake(_cameraShaker);
        HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
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