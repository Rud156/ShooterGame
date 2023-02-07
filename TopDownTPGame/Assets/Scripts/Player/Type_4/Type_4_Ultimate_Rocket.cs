#region

using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using Player.UI;
using UnityEngine;

#endregion

public class Type_4_Ultimate_Rocket : Ability
{
    private const float MaxUltimatePercent = 100;

    [Header("Prefabs")]
    [SerializeField] private GameObject _rocketPrefab;

    [Header("Components")]
    [SerializeField] private BaseShootController _baseShootController;

    [Header("Ultimate Data")]
    [SerializeField] private float _ultimateChargeRate;

    private GameObject _rocketObject;
    private bool _abilityEnd;

    private float _currentUltimatePercent;

    #region Ability Functions

    public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController) && _currentUltimatePercent >= MaxUltimatePercent;

    public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

    public override void AbilityUpdate(BasePlayerController playerController)
    {
        if (_rocketObject == null)
        {
            var shootPosition = _baseShootController.GetShootPosition();
            var shootPoint = _baseShootController.GetShootPoint();

            var rocket = Instantiate(_rocketPrefab, shootPosition, Quaternion.LookRotation(shootPoint.forward));
            var rocketRb = rocket.GetComponent<Rigidbody>();

            rocket.transform.SetParent(shootPoint);
            rocketRb.isKinematic = true;
            _rocketObject = rocket;
        }
        else
        {
            var direction = _baseShootController.GetShootLookDirection();
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

    public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;

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

        if (_currentUltimatePercent < MaxUltimatePercent)
        {
            _currentUltimatePercent += Time.fixedDeltaTime * _ultimateChargeRate;
            if (_currentUltimatePercent > MaxUltimatePercent)
            {
                _currentUltimatePercent = MaxUltimatePercent;
            }
        }
    }

    #endregion Unity Functions

    #region Utils

    private void DisplayUltimateToHUD() => PlayerAbilityDisplay.Instance.UpdateCooldownPercent(AbilityTrigger.Ultimate, _currentUltimatePercent, MaxUltimatePercent);

    #endregion Utils
}