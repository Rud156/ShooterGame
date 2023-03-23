#region

using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;

#endregion

public class Type_4_Ultimate_Rocket : Ability
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _rocketPrefab;

    [Header("Components")]
    [SerializeField] private PlayerBaseShootController _shootController;

    [Header("Ultimate Data")]
    [SerializeField] private float _ultimateChargeRate;
    [SerializeField] private float _windUpTime;

    private float _currentWindUpTime;
    private bool _abilityEnd;

    private float _currentUltimatePercent;

    #region Ability Functions

    public override bool AbilityCanStart(BasePlayerController playerController) =>
        base.AbilityCanStart(playerController) && _currentUltimatePercent >= PlayerStaticData.MaxUltimatePercent;

    public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

    public override void AbilityUpdate(BasePlayerController playerController)
    {
        _currentWindUpTime -= Time.fixedDeltaTime;
        if (_currentWindUpTime <= 0)
        {
            var shootPosition = _shootController.GetShootPosition();
            var direction = _shootController.GetShootLookDirection();

            var rocket = Instantiate(_rocketPrefab, shootPosition, Quaternion.LookRotation(direction));
            var rocketProjectile = rocket.GetComponent<RocketProjectile>();

            rocketProjectile.LaunchProjectile(direction);

            _currentUltimatePercent = 0;
            _abilityEnd = true;

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

        if (_currentUltimatePercent < PlayerStaticData.MaxUltimatePercent)
        {
            _currentUltimatePercent += Time.fixedDeltaTime * _ultimateChargeRate;
            if (_currentUltimatePercent > PlayerStaticData.MaxUltimatePercent)
            {
                _currentUltimatePercent = PlayerStaticData.MaxUltimatePercent;
            }
        }
    }

    #endregion Unity Functions

    #region Utils

    private void DisplayUltimateToHUD()
    {
        HUD_PlayerAbilityDisplay.Instance.UpdateTimer(AbilityTrigger.Ultimate, $"{_currentUltimatePercent:0.0} %", true);

        var overlayPercent = _currentUltimatePercent >= PlayerStaticData.MaxUltimatePercent ? 0 : 1;
        HUD_PlayerAbilityDisplay.Instance.UpdateOverlay(AbilityTrigger.Ultimate, overlayPercent);
    }

    #endregion Utils
}