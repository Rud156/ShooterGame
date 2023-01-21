    #region

using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_2
{
    public class Type_2_Secondary_IceWall : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _iceWallPrefab;

        [Header("Component")]
        [SerializeField] private BaseShootController _shootController;

        [Header("Spawn Data")]
        [SerializeField] private float _windUpTime;
        [SerializeField] private Vector3 _spawnOffset;

        private bool _abilityEnd;
        private float _currentWindUpTime;

        public override bool AbilityCanStart(BasePlayerController playerController) => playerController.IsGrounded && _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentWindUpTime -= Time.fixedDeltaTime;
            if (_currentWindUpTime > 0)
            {
                return;
            }

            var spawnPosition = _shootController.GetShootPosition();
            var forward = transform.forward;
            var right = transform.right;

            spawnPosition += forward * _spawnOffset.z + right * _spawnOffset.x;
            spawnPosition.y += _spawnOffset.y;

            Instantiate(_iceWallPrefab, spawnPosition, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
            _abilityEnd = true;

            _currentCooldownDuration = _cooldownDuration;
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController)
        {
            _currentWindUpTime = _windUpTime;
            _abilityEnd = false;
        }
    }
}