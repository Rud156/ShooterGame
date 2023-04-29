#region

using System.Collections.Generic;
using HealthSystem;
using Player.Base;
using Player.Common;
using UI.DisplayManagers.Player;
using UnityEngine;
using Utils.Structs;
using World;

#endregion

namespace Player.Type_3
{
    public class Type_3_Secondary_DarkSlash : Ability
    {
        [Header("Components")]
        [SerializeField] private GameObject _slashSword;
        [SerializeField] private SimpleDamageTrigger _swordDamageTrigger;
        [SerializeField] private Type_3_Ultimate_DarkPulseParanoiaAbility _type3Ultimate;
        [SerializeField] private Animator _playerAnimator;

        [Header("Anim Data")]
        [SerializeField] private int _animMinIndex;
        [SerializeField] private int _animMaxIndex;
        [SerializeField] private List<SwordPositionRotation> _swordPositionRotations;

        [Header("Slash Data")]
        [SerializeField] private float _slashDuration;

        [Header("Ultimate Charge Data")]
        [SerializeField] private int _ultimateChargeAmount;

        private float _currentTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            _currentTime += WorldTimeManager.Instance.FixedUpdateTime;
            if (_currentTime >= _slashDuration)
            {
                _abilityEnd = true;
                _currentCooldownDuration = _cooldownDuration;

                _slashSword.SetActive(false);
                _playerAnimator.SetInteger(PlayerStaticData.Type_3_Secondary, 0);
            }
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            _abilityEnd = true;
            _currentTime = 0;
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            var animIndex = Random.Range(_animMinIndex, _animMaxIndex + 1);
            _playerAnimator.SetInteger(PlayerStaticData.Type_3_Secondary, animIndex);

            _slashSword.SetActive(true);
            _slashSword.transform.localPosition = _swordPositionRotations[animIndex - 1].Position;
            _slashSword.transform.localRotation = Quaternion.Euler(_swordPositionRotations[animIndex - 1].Rotation);

            _currentTime = 0;
            _abilityEnd = false;

            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
        }

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            base.UnityStartDelegate(playerController);
            _swordDamageTrigger.SetCollisionCallback(HandleSwordCollisionCallback);
        }

        #endregion Unity Functions

        #region External Functions

        private void HandleSwordCollisionCallback(Collider other) => _type3Ultimate.AddUltimateCharge(_ultimateChargeAmount);

        #endregion External Functions
    }
}