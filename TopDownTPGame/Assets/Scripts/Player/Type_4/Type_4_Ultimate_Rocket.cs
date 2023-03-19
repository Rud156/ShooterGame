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

    private GameObject _rocketObject;
    private bool _abilityEnd;

    private float _currentUltimatePercent;

    #region Ability Functions

    public override bool AbilityCanStart(BasePlayerController playerController) =>
        base.AbilityCanStart(playerController) && _currentUltimatePercent >= PlayerStaticData.MaxUltimatePercent;

    public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

    public override void AbilityUpdate(BasePlayerController playerController)
    {
        if (_rocketObject == null)
        {
            var shootPosition = _shootController.GetShootPosition();
            var shootPoint = _shootController.GetShootPoint();

            var rocket = Instantiate(_rocketPrefab, shootPosition, Quaternion.LookRotation(shootPoint.forward));
            var rocketRb = rocket.GetComponent<Rigidbody>();

            rocket.transform.SetParent(shootPoint);
            rocketRb.isKinematic = true;
            _rocketObject = rocket;
        }
        else
        {
            var direction = _shootController.GetShootLookDirection();
            var rocket = _rocketObject.GetComponent<RocketProjectile>();
            var rocketRb = _rocketObject.GetComponent<Rigidbody>();

            _rocketObject.transform.SetParent(null);
            _rocketObject.transform.rotation = Quaternion.LookRotation(direction);
            rocketRb.isKinematic = false;
            rocket.LaunchProjectile(direction);
            _rocketObject = null;
            _currentUltimatePercent = 0;
        }

        _abilityEnd = true;
    }

    public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

    public override void StartAbility(BasePlayerController playerController)
    {
        _abilityEnd = false;
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