using Player.Abilities;
using Player.Core;
using System;
using UI.Player;
using UnityEngine;

namespace Player.Type_2
{
    public class Type_2_Primary_WaterStab : AbilityBase
    {
        private static readonly int Type_2_PrimaryAnimParam = Animator.StringToHash("Type_2_Primary");

        [Header("Components")]
        [SerializeField] private GameObject _rightHandSword;

        [Header("Anim Data")]
        [SerializeField] private SwordAttackData _leftSlashData;
        [SerializeField] private SwordAttackData _rightSlashData;
        [SerializeField] private SwordAttackData _stabData;

        [Header("Slash Data")]
        [SerializeField] private float _slashStateCooldownDuration;

        private WaterSwordState _waterSwordState;
        private float _waterStateCooldown;
        private float _slashTimer;

        private bool _abilityMarkedForEnd;

        #region Core Ability Functions

        public override void AbilityStart(PlayerController playerController)
        {
            base.AbilityStart(playerController);
            _abilityMarkedForEnd = false;
            TriggerSwordSlash();
            HUD_PlayerAbilityDisplay.Instance.TriggerAbilityFlashAndScale(_abilityTrigger);
        }

        public override void AbilityFixedUpdate(PlayerController playerController, float fixedDeltaTime)
        {
        }

        public override void AbilityUpdate(PlayerController playerController, float deltaTime)
        {
        }

        public override void AbilityEnd(PlayerController playerController)
        {
            _abilityMarkedForEnd = true;
            _playerAnimator.SetInteger(Type_2_PrimaryAnimParam, 0);
            _rightHandSword.SetActive(false);
        }

        #endregion Core Ability Functions

        #region Ability Conditions

        public override bool AbilityCanStart(PlayerController playerController) => base.AbilityCanStart(playerController) && _slashTimer <= 0;

        public override bool AbilityNeedsToEnd(PlayerController playerController) => _abilityMarkedForEnd;

        #endregion Ability Conditions

        #region Getters

        public override Vector3 GetMovementData() => Vector3.zero;

        #endregion Getters

        #region Unity Function Delegates

        public override void UnityStartDelegate(PlayerController playerController)
        {
            base.UnityStartDelegate(playerController);
            SetState(WaterSwordState.LeftSlash);
            _waterStateCooldown = 0;
        }

        public override void UnityFixedUpdateDelegate(PlayerController playerController, float fixedDeltaTime)
        {
            base.UnityFixedUpdateDelegate(playerController, fixedDeltaTime);
            UpdateSlashTimer(fixedDeltaTime);

            if (_waterStateCooldown <= 0)
            {
                return;
            }

            _waterStateCooldown -= fixedDeltaTime;
            if (_waterStateCooldown <= 0)
            {
                switch (_waterSwordState)
                {
                    case WaterSwordState.LeftSlash:
                        _waterStateCooldown = 0;
                        break;

                    case WaterSwordState.RightSlash:
                        SetState(WaterSwordState.LeftSlash);
                        _waterStateCooldown = _slashStateCooldownDuration;
                        break;

                    case WaterSwordState.Stab:
                        SetState(WaterSwordState.RightSlash);
                        _waterStateCooldown = _slashStateCooldownDuration;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion Unity Function Delegates

        #region Sword Slash Control

        private void TriggerSwordSlash()
        {
            var swordData = _waterSwordState switch
            {
                WaterSwordState.LeftSlash => _leftSlashData,
                WaterSwordState.RightSlash => _rightSlashData,
                WaterSwordState.Stab => _stabData,
                _ => _stabData,
            };

            _slashTimer = swordData.SlashDuration;
            _playerAnimator.SetInteger(Type_2_PrimaryAnimParam, swordData.AnimIndex);
            _rightHandSword.SetActive(true);
            _rightHandSword.transform.localPosition = swordData.Position;
            _rightHandSword.transform.localRotation = Quaternion.Euler(swordData.Rotation);
            _playerController.ForcePlayerLookToMousePosition();
        }

        private void UpdateSlashTimer(float fixedDeltaTime)
        {
            if (_slashTimer <= 0)
            {
                return;
            }

            _slashTimer -= fixedDeltaTime;
            if (_slashTimer <= 0)
            {
                IncrementCurrentState();
                _abilityMarkedForEnd = true;
            }
        }

        #endregion Sword Slash Control

        #region Ability State Updates

        private void SetState(WaterSwordState waterSwordState) => _waterSwordState = waterSwordState;

        private void IncrementCurrentState()
        {
            switch (_waterSwordState)
            {
                case WaterSwordState.LeftSlash:
                    SetState(WaterSwordState.RightSlash);
                    _waterStateCooldown = _slashStateCooldownDuration;
                    break;

                case WaterSwordState.RightSlash:
                    SetState(WaterSwordState.Stab);
                    _waterStateCooldown = _slashStateCooldownDuration;
                    break;

                case WaterSwordState.Stab:
                    SetState(WaterSwordState.LeftSlash);
                    _waterStateCooldown = _slashStateCooldownDuration;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion Ability State Updates

        #region Structs

        [Serializable]
        public struct SwordAttackData
        {
            public int AnimIndex;
            public float SlashDuration;
            public Vector3 Position;
            public Vector3 Rotation;
        }

        #endregion Structs

        #region Enums

        private enum WaterSwordState
        {
            LeftSlash,
            RightSlash,
            Stab
        }

        #endregion Enums
    }
}