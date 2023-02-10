#region

using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Misc;

#endregion

public class Type_5_Ultimate_Shield : Ability
{
    [Header("Shield Data")]
    [SerializeField] private float _shieldDeployRadius;
    [SerializeField] private float _shieldDuration;
    [SerializeField] private LayerMask _shieldDeployMask;

    private Collider[] _hitColliders = new Collider[StaticData.MaxCollidersCheck];

    private bool _abilityEnd;

    public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController);

    public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

    public override void AbilityUpdate(BasePlayerController playerController)
    {
        var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _shieldDeployRadius, _hitColliders, _shieldDeployMask);
        for (var i = 0; i < targetsHit; i++)
        {
            // Do not target itself
            if (_hitColliders[i] == null)
            {
                continue;
            }

            // TODO: Also check team here...
            if (_hitColliders[i].TryGetComponent(out BasePlayerController targetController))
            {
                // TODO: Enable Engineer Shield as an Ability
            }
        }

        _abilityEnd = true;
    }

    public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

    public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;
}