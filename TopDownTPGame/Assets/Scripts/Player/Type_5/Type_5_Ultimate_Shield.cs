using Player.Base;
using Player.Common;
using UnityEngine;

public class Type_5_Ultimate_Shield : Ability
{
    private const int MaxCollidersCheck = 10;

    [Header("Shield Data")]
    [SerializeField] private float _shieldDeployRadius;
    [SerializeField] private float _shieldDuration;
    [SerializeField] private LayerMask _shieldDeployMask;

    private bool _abilityEnd;

    public override bool AbilityCanStart(BasePlayerController playerController) => true;

    public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

    public override void AbilityUpdate(BasePlayerController playerController)
    {
        Collider[] hitColliders = new Collider[MaxCollidersCheck];
        int targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _shieldDeployRadius, hitColliders, _shieldDeployMask);
        for (int i = 0; i < targetsHit; i++)
        {
            // Do not target itself
            if (hitColliders[i] == null)
            {
                continue;
            }

            // TODO: Also check team here...
            if (hitColliders[i].TryGetComponent(out BasePlayerController targetController))
            {
                targetController.CharacterEnableEngineerShield(_shieldDuration);
            }
        }

        _abilityEnd = true;
    }

    public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

    public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;
}