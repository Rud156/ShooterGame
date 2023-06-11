#region

using Player.Base;
using Player.Common;
using UI.DisplayManagers.Player;
using UnityEngine;
using Utils.Common;
using World;

#endregion

namespace Player.Type_2
{
    public class Type_2_Secondary_IceWall : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _iceWallPrefab;

        [Header("Component")]
        [SerializeField] private OwnerData _ownerIdData;
        [SerializeField] private PlayerBaseShootController _shootController;
        [SerializeField] private Animator _playerAnimator;

        [Header("Spawn Data")]
        [SerializeField] private float _windUpTime;
        [SerializeField] private Vector3 _spawnOffset;

        private bool _abilityEnd;
        private float _currentWindUpTime;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) =>
            base.AbilityCanStart(playerController) && playerController.IsGrounded && _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentWindUpTime -= WorldTimeManager.Instance.FixedUpdateTime;
            if (_currentWindUpTime > 0)
            {
                return;
            }

            var spawnPosition = _shootController.GetShootPosition();
            var characterTransform = transform;
            var forward = characterTransform.forward;
            var right = characterTransform.right;

            spawnPosition += forward * _spawnOffset.z + right * _spawnOffset.x;
            spawnPosition.y += _spawnOffset.y;

            var iceWall = Instantiate(_iceWallPrefab, spawnPosition, Quaternion.Euler(0, characterTransform.rotation.eulerAngles.y, 0));
            var ownerData = iceWall.GetComponent<OwnerData>();

            ownerData.OwnerId = _ownerIdData.OwnerId;
            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);

            _playerAnimator.SetTrigger(PlayerStaticData.Type_2_Secondary);
            _abilityEnd = true;
            _currentCooldownDuration = _cooldownDuration;
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController)
        {
            _currentWindUpTime = _windUpTime;
            _abilityEnd = false;
        }

        #endregion Ability Functions
    }
}