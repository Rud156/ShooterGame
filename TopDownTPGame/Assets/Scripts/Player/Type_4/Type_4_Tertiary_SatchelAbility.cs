#region

using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UI.DisplayManagers.Player;
using UnityEngine;
using Utils.Common;

#endregion

namespace Player.Type_4
{
    public class Type_4_Tertiary_SatchelAbility : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _satchelPrefab;

        [Header("Components")]
        [SerializeField] private OwnerData _ownerIdData;
        [SerializeField] private PlayerBaseShootController _shootController;
        [SerializeField] private Animator _playerAnimator;

        [Header("Dash Charges")]
        [SerializeField] private int _satchelCount;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private bool _abilityEnd;

        private SatchelNade _satchelObject;
        private int _currentSatchelsLeft;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) =>
            base.AbilityCanStart(playerController) && (_satchelObject != null || _currentSatchelsLeft > 0);

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController) => InitialSatchelActivation(playerController);

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController) => _abilityEnd = false;

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            OnAbilityCooldownComplete += HandleCooldownComplete;
            base.UnityStartDelegate(playerController);

            _currentSatchelsLeft = _satchelCount;
        }

        private void OnDestroy() => OnAbilityCooldownComplete -= HandleCooldownComplete;

        public override void UnityUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityUpdateDelegate(playerController);
            UpdateDashCountChanged();
        }

        #endregion Unity Functions

        #region Ability Updates

        private void InitialSatchelActivation(BasePlayerController playerController)
        {
            if (_satchelObject == null && _currentSatchelsLeft > 0)
            {
                var shootPoint = _shootController.GetVirtualShootPosition();
                var direction = _shootController.GetShootLookDirection();
                if (_debugIsActive)
                {
                    Debug.DrawRay(shootPoint, direction * 50, Color.red, _debugDisplayDuration);
                }

                var satchel = Instantiate(_satchelPrefab, shootPoint, Quaternion.identity);
                var satchelNade = satchel.GetComponent<SatchelNade>();
                var ownerData = satchel.GetComponent<OwnerData>();

                satchelNade.LaunchProjectile(direction);
                ownerData.OwnerId = _ownerIdData.OwnerId;
                _playerAnimator.SetTrigger(PlayerStaticData.Type_4_Tertiary);
                HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);

                _satchelObject = satchelNade;
                _abilityEnd = true;

                _currentSatchelsLeft -= 1;
                if (_currentSatchelsLeft <= 0)
                {
                    _currentCooldownDuration = _cooldownDuration;
                }
            }
            else if (_satchelObject != null)
            {
                _satchelObject.ProjectileDestroy();
                _satchelObject = null;
                _abilityEnd = true;
            }
        }

        private void HandleCooldownComplete()
        {
            _currentSatchelsLeft = Mathf.Clamp(_currentSatchelsLeft + 1, 0, _satchelCount);
            if (_currentSatchelsLeft < _satchelCount)
            {
                _currentCooldownDuration = _cooldownDuration;
            }
        }

        private void UpdateDashCountChanged() => HUD_PlayerAbilityDisplay.Instance.UpdateCounter(AbilityTrigger.Tertiary, $"{_currentSatchelsLeft}", true);

        #endregion Ability Updates
    }
}