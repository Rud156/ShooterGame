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
            GameObject rocket = Instantiate(_rocketPrefab, _shootPoint.position, Quaternion.identity);
            Rigidbody rocketRb = rocket.GetComponent<Rigidbody>();

            rocket.transform.SetParent(_shootPoint);
            rocketRb.isKinematic = true;
            _rocketObject = rocket;
        }
        else
        {
            Vector3 direction = _cameraHolder.forward;
            RocketProjectile rocket = _rocketObject.GetComponent<RocketProjectile>();
            Rigidbody rocketRb = _rocketObject.GetComponent<Rigidbody>();

            _rocketObject.transform.SetParent(null);
            rocketRb.isKinematic = false;
            rocket.LaunchProjectile(direction);
            _rocketObject = null;

            Vector3 knockbackDirection = -_cameraHolder.forward * _knockbackVelocity;
            playerController.KnockbackCharacter(_knockbackDuration, knockbackDirection);
        }

        _abilityEnd = true;
    }

    public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

    public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;
}