#region

using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UI.DisplayManagers.Player;
using UnityEngine;

#endregion

namespace Player.Type_4
{
    public class Type_4_Tertiary_SatchelAbility : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _satchelPrefab;
        [SerializeField] private GameObject _rotatingShootPrefab;
        [SerializeField] private GameObject _staticShootPrefab;

        [Header("Components")]
        [SerializeField] private Transform _parent;
        [SerializeField] private Transform _cinemachineFollowTarget;
        [SerializeField] private PlayerBaseShootController _shootController;
        [SerializeField] private Animator _playerAnimator;

        [Header("Dash Charges")]
        [SerializeField] private int _satchelCount;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private GameObject _staticShootPointHolder;
        private GameObject _rotatingShootPointHolder;
        private Transform _staticShootPoint;
        private Transform _rotatingShootPoint;

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

            var spawnPosition = _parent.position;
            _rotatingShootPointHolder = Instantiate(_rotatingShootPrefab, spawnPosition, Quaternion.identity, _parent);
            _staticShootPointHolder = Instantiate(_staticShootPrefab, spawnPosition, Quaternion.identity, _parent);

            _rotatingShootPoint = _rotatingShootPointHolder.transform.Find("RotatingShootPoint");
            _staticShootPoint = _staticShootPointHolder.transform.Find("StaticShootPoint");
            _currentSatchelsLeft = _satchelCount;
        }

        private void OnDestroy()
        {
            Destroy(_rotatingShootPointHolder);
            Destroy(_staticShootPointHolder);
            OnAbilityCooldownComplete -= HandleCooldownComplete;
        }

        public override void UnityUpdateDelegate(BasePlayerController playerController)
        {
            base.UnityUpdateDelegate(playerController);
            UpdateDashCountChanged();

            _rotatingShootPointHolder.transform.localRotation = _cinemachineFollowTarget.localRotation;
        }

        #endregion Unity Functions

        #region Ability Updates

        private void InitialSatchelActivation(BasePlayerController playerController)
        {
            if (_satchelObject == null && _currentSatchelsLeft > 0)
            {
                var shootPointTransform = playerController.IsGrounded ? _staticShootPoint : _rotatingShootPoint;
                var shootPoint = shootPointTransform.position;
                var direction = _shootController.GetShootLookDirection();
                if (_debugIsActive)
                {
                    Debug.DrawRay(shootPoint, direction * 50, Color.red, _debugDisplayDuration);
                }

                var satchel = Instantiate(_satchelPrefab, shootPoint, Quaternion.identity);
                var satchelNade = satchel.GetComponent<SatchelNade>();

                satchelNade.LaunchProjectile(direction);
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