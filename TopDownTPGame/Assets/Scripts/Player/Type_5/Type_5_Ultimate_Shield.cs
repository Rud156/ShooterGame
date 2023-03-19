#region

using Player.Base;
using Player.Common;
using Player.Type_5;
using UI.Player;
using UnityEngine;

#endregion

public class Type_5_Ultimate_Shield : Ability
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _shieldPrefab;

    [Header("Shield Data")]
    [SerializeField] private float _shieldDeployRadius;
    [SerializeField] private LayerMask _shieldDeployMask;

    [Header("Ultimate Data")]
    [SerializeField] private float _windUptime;
    [SerializeField] private float _ultimateChargeRate;

    private float _currentUltimatePercent;
    private float _currentWindUpTime;
    private bool _abilityEnd;

    private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];

    #region Ability Functions

    public override bool AbilityCanStart(BasePlayerController playerController) =>
        base.AbilityCanStart(playerController) && _currentUltimatePercent >= PlayerStaticData.MaxUltimatePercent;

    public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

    public override void AbilityUpdate(BasePlayerController playerController)
    {
        _currentWindUpTime -= Time.fixedDeltaTime;
        if (_currentWindUpTime <= 0)
        {
            var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _shieldDeployRadius, _hitColliders, _shieldDeployMask);
            for (var i = 0; i < targetsHit; i++)
            {
                if (_hitColliders[i].TryGetComponent(out BasePlayerController targetController))
                {
                    var targetTransform = _hitColliders[i].transform;
                    var shield = Instantiate(_shieldPrefab, targetTransform.position, Quaternion.identity, targetTransform);
                    var shieldAbility = shield.GetComponent<Type_5_Ultimate_EngineerShield>();
                    targetController.CheckAndAddExternalAbility(shieldAbility);
                }
            }

            _abilityEnd = true;
        }
    }

    public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

    public override void StartAbility(BasePlayerController playerController)
    {
        _currentWindUpTime = _windUptime;
        _currentUltimatePercent = 0;
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