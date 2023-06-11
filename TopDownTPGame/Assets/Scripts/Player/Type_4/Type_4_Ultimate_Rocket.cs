#region

using Ability_Scripts.Projectiles;
using CustomCamera;
using Player.Base;
using Player.Common;
using Player.Type_4;
using UI.DisplayManagers.Player;
using UnityEngine;
using Utils.Common;
using World;

#endregion

public class Type_4_Ultimate_Rocket : Ability
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _rocketPrefab;

    [Header("Components")]
    [SerializeField] private OwnerData _ownerIdData;
    [SerializeField] private PlayerBaseShootController _shootController;
    [SerializeField] private Type_4_DroneController _droneController;

    [Header("Ultimate Data")]
    [SerializeField] private float _windUpTime;
    [SerializeField] private int _ultimateChargePerSec;
    [SerializeField] private int _maxUltimateChargeAmount;

    [Header("Camera Data")]
    [SerializeField] private CameraShaker _cameraShaker;

    [Header("Debug")]
    [SerializeField] private bool _debugIsActive;

    private float _currentWindUpTime;
    private bool _abilityEnd;

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
            var shootPosition = _shootController.GetShootPosition();
            var direction = _shootController.GetShootLookDirection();

            var rocket = Instantiate(_rocketPrefab, shootPosition, Quaternion.LookRotation(direction));
            var rocketProjectile = rocket.GetComponent<RocketProjectile>();
            var ownerData = rocket.GetComponent<OwnerData>();

            rocketProjectile.LaunchProjectile(direction);
            ownerData.OwnerId = _ownerIdData.OwnerId;

            _currentUltimateAmount = 0;
            _abilityEnd = true;

            _droneController.KnockbackDrone(PlayerStaticData.Type_4_UltimateDroneKnockbackMultiplier);
            CustomCameraController.Instance.StartShake(_cameraShaker);
            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
        }
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