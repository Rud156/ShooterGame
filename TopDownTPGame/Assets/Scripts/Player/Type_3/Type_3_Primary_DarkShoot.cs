using AbilityScripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Input;

namespace Player.Type_3
{
    public class Type_3_Primary_DarkShoot : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _projectilePrefab;

        [Header("Components")]
        [SerializeField] private Transform _cameraHolder;

        [Header("Shoot Data")]
        [SerializeField] private float _fireRate;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private float _spawnQuaternionOffset;
        [SerializeField] private float _shootAngle;
        [SerializeField] private int _totalProjectiles;

        private float _nextFireTime;
        private bool _abilityEnd;

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (Time.time >= _nextFireTime)
            {
                _nextFireTime = Time.time + _fireRate;
                Vector3 spawnPosition = _shootPoint.position;

                bool isEven = _totalProjectiles % 2 == 0;
                float startAngle;
                if (isEven)
                {
                    int halfProjectiles = _totalProjectiles / 2;
                    startAngle = (halfProjectiles - 1) * _shootAngle + (_shootAngle / 2);
                }
                else
                {
                    int halfProjectiles = Mathf.FloorToInt(_totalProjectiles / 2);
                    startAngle = halfProjectiles * _shootAngle;
                }
                startAngle = -startAngle;

                for (int i = 0; i < _totalProjectiles; i++)
                {
                    GameObject projectile = Instantiate(_projectilePrefab, spawnPosition, Quaternion.Euler(0, startAngle, 0));
                    SimpleOneShotForwardProjectile simpleProj = projectile.GetComponent<SimpleOneShotForwardProjectile>();

                    Quaternion angleDirection = Quaternion.Euler(0, startAngle - _spawnQuaternionOffset, 0);
                    Vector3 secondaryDirection = angleDirection * _cameraHolder.forward + angleDirection * _cameraHolder.right;
                    simpleProj.LaunchProjectile(secondaryDirection);

                    startAngle += _shootAngle;
                }
            }

            PlayerInputKey inputKey = playerController.GetPrimaryAbilityKey();
            if (inputKey.keyReleasedThisFrame || !inputKey.keyPressed)
            {
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;
    }
}