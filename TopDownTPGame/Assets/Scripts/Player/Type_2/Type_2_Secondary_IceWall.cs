using Player.Base;
using Player.Common;
using UnityEngine;

namespace Player.Type_2
{
    public class Type_2_Secondary_IceWall : Ability
    {
        [Header("Components")]
        [SerializeField] private GameObject _iceWallPrefab;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Vector3 _spawnOffset;

        private bool _abilityEnd;

        public override bool AbilityCanStart(BasePlayerController playerController) => playerController.IsGrounded;

        public override bool AbilityNeedsToEnd() => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            Vector3 spawnPosition = _spawnPoint.position;
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            spawnPosition += forward * _spawnOffset.z + right * _spawnOffset.x;
            spawnPosition.y += _spawnOffset.y;

            Instantiate(_iceWallPrefab, spawnPosition, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
            _abilityEnd = true;
        }

        public override void EndAbility() => _abilityEnd = true;

        public override void StartAbility()
        {
            // Don't do anything here...
        }
    }
}