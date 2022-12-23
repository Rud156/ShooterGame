using AbilityScripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Input;

namespace Player.Type_1
{
    public class Type_1_Secondary_ChargedShoot : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _chargedObjectPrefab;

        [Header("Components")]
        [SerializeField] private Transform _cameraHolder;

        [Header("Charged Shoot Data")]
        [SerializeField] private float _minChargeDuration;
        [SerializeField] private float _maxChargeDuration;
        [SerializeField] private Transform _shootPoint;

        private bool _abilityEnd;
        private float _currentChargeTime;

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            PlayerInputKey key = playerController.GetSecondaryAbilityKey();
            if (key.keyReleasedThisFrame || !key.keyPressed || _currentChargeTime >= _maxChargeDuration)
            {
                if (_currentChargeTime >= _minChargeDuration)
                {
                    Vector3 spawnPosition = _shootPoint.position;
                    Vector3 direction = _cameraHolder.forward;

                    GameObject projectile = Instantiate(_chargedObjectPrefab, spawnPosition, Quaternion.identity);
                    SimpleOneShotForwardProjectile simpleProj = projectile.GetComponent<SimpleOneShotForwardProjectile>();
                    simpleProj.LaunchProjectile(direction);
                }

                _abilityEnd = true;
            }
            else
            {
                _currentChargeTime += Time.fixedDeltaTime;
            }
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            _abilityEnd = true;
            _currentChargeTime = 0;
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            _abilityEnd = false;
            _currentChargeTime = 0;
        }
    }
}