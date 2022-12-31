using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;

public class Type_4_Ultimate_Rocket : Ability
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _rocketPrefab;

    [Header("Components")]
    [SerializeField] private Transform _cameraHolder;
    [SerializeField] private Transform _shootPoint;

    [Header("Rocket Data")]
    [SerializeField] private float _knockbackDuration;
    [SerializeField] private float _knockbackVelocity;

    private GameObject _rocketObject;
    private bool _abilityEnd;

    public override bool AbilityCanStart(BasePlayerController playerController) => true;

    public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

    public override void AbilityUpdate(BasePlayerController playerController)
    {
        if (_rocketObject == null)
        {
            var rocket = Instantiate(_rocketPrefab, _shootPoint.position, Quaternion.identity);
            var rocketRb = rocket.GetComponent<Rigidbody>();

            rocket.transform.SetParent(_shootPoint);
            rocketRb.isKinematic = true;
            _rocketObject = rocket;
        }
        else
        {
            var direction = _cameraHolder.forward;
            var rocket = _rocketObject.GetComponent<RocketProjectile>();
            var rocketRb = _rocketObject.GetComponent<Rigidbody>();

            _rocketObject.transform.SetParent(null);
            rocketRb.isKinematic = false;
            rocket.LaunchProjectile(direction);
            _rocketObject = null;

            var knockbackVelocity = -_cameraHolder.forward * _knockbackVelocity;
            playerController.KnockbackCharacter(_knockbackDuration, knockbackVelocity);
        }

        _abilityEnd = true;
    }

    public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

    public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;
}