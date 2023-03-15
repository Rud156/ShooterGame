#region

using Player.Base;
using Player.Common;
using UI.Player;
using UnityEngine;

#endregion

namespace Player.Type_3
{
    public class Type_3_Secondary_DarkSlash : Ability
    {
        [Header("Components")]
        [SerializeField] private GameObject _slashSword;

        [Header("Slash Data")]
        [SerializeField] private float _slashDuration;

        private float _currentTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentTime += Time.fixedDeltaTime;
            if (_currentTime >= _slashDuration)
            {
                _slashSword.SetActive(false);
                _abilityEnd = true;
                _currentCooldownDuration = _cooldownDuration;
            }
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            _abilityEnd = true;
            _currentTime = 0;
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            _slashSword.SetActive(true);
            _currentTime = 0;
            _abilityEnd = false;

            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlash(_abilityTrigger);
        }

        #endregion Ability Functions
    }
}