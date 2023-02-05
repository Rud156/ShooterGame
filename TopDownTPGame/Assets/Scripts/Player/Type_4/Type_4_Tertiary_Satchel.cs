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

        private SatchelNade _satchelObject;
        private bool _abilityEnd;

        private int _currentSatchelsLeft;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController) && _currentSatchelsLeft > 0;

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
                var shootPoint = _shootController.GetShootPosition();

                var satchel = Instantiate(_satchelPrefab, shootPoint, Quaternion.identity);
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

        private void UpdateDashCountChanged() => PlayerAbilityDisplay.Instance.UpdateStackCount(AbilityTrigger.Tertiary, _currentSatchelsLeft);

        #endregion Ability Updates
    }
}