#region

using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using Player.UI;
using UnityEngine;

#endregion

namespace Player.Type_4
{
    public class Type_4_Tertiary_Satchel : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _satchelPrefab;

        [Header("Components")]
        [SerializeField] private AbilityPrefabInitializer _prefabInit;
        [SerializeField] private Transform _cameraHolder;
        [SerializeField] private BaseShootController _shootController;

        [Header("Dash Charges")]
        [SerializeField] private int _satchelCount;

        [Header("Post Start Filled")]
        [SerializeField] private Transform _orbitShootPoint;
        [SerializeField] private Transform _staticShootPoint;

        private SatchelNade _satchelObject;
        private bool _abilityEnd;

        private int _currentSatchelsLeft;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => _currentSatchelsLeft > 0;

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

            _prefabInit.AbilityPrefabInit();
            _orbitShootPoint = transform.Find("CameraHolder/Type_4_CameraHolderPrefab(Clone)/BelowShootPoint");
            _staticShootPoint = transform.Find("Type_4_NormalPrefab(Clone)/BelowShootPoint");

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
            if (_satchelObject == null)
            {
                var direction = _shootController.GetShootLookDirection();

                var shootPosition = playerController.IsGrounded ? _staticShootPoint.position : _orbitShootPoint.position;
                var satchel = Instantiate(_satchelPrefab, shootPosition, Quaternion.identity);
                var satchelNade = satchel.GetComponent<SatchelNade>();
                satchelNade.LaunchProjectile(direction);

                _satchelObject = satchelNade;
                _abilityEnd = true;

                _currentSatchelsLeft -= 1;
                if (_currentSatchelsLeft <= 0)
                {
                    _currentCooldownDuration = _cooldownDuration;
                }
            }
            else
            {
                _satchelObject.LaunchPlayersWithSatchel();
                _satchelObject.ProjectileDestroy();
                _satchelObject = null;
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

        private void UpdateDashCountChanged() => PlayerAbilityDisplay.Instance.UpdateStackCount(AbilityTrigger.Tertiary, _currentSatchelsLeft);

        #endregion Ability Updates
    }
}