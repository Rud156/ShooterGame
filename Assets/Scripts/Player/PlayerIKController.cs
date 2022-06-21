using UnityEngine;
using Weapons;

namespace Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerIKController : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponsInventoryController m_playerWeaponsController;
        [SerializeField] private BasePlayerController m_playerController;

        private Animator m_animator;

        private bool m_handIKActive;
        private WeaponController m_currentWeaponController;

        #region Unity Functions

        private void Start()
        {
            m_playerWeaponsController.OnWeaponPickup += HandleWeaponPickup;
            m_playerWeaponsController.OnWeaponDrop += ClearHandIK;
            m_playerController.OnPlayerStatePushed += HandlePlayerStatePushed;
            m_playerController.OnPlayerStatePopped += HandlePlayerStatePopped;

            m_animator = GetComponent<Animator>();
        }

        private void OnDestroy()
        {
            m_playerWeaponsController.OnWeaponPickup -= HandleWeaponPickup;
            m_playerWeaponsController.OnWeaponDrop -= ClearHandIK;
            m_playerController.OnPlayerStatePushed -= HandlePlayerStatePushed;
            m_playerController.OnPlayerStatePopped -= HandlePlayerStatePopped;
        }

        private void OnAnimatorIK()
        {
            if (m_handIKActive)
            {
                // Left Hand
                Vector3 leftHandPosition = m_currentWeaponController.GetFrontPoint().position;
                Quaternion leftHandRotation = m_currentWeaponController.GetFrontPoint().rotation;

                m_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                m_animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPosition);
                m_animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                m_animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandRotation);

                // Right Hand
                Vector3 rightHandPosition = m_currentWeaponController.GetTriggerPoint().position;
                Quaternion rightHandRotation = m_currentWeaponController.GetTriggerPoint().rotation;

                m_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                m_animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandPosition);
                m_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                m_animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandRotation);
            }
            else
            {
                m_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                m_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                m_animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                m_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            }
        }

        #endregion Unity Functions

        #region Player State

        private void HandlePlayerStatePushed(BasePlayerController.PlayerState pushedState)
        {
            switch (pushedState)
            {
                case BasePlayerController.PlayerState.Idle:
                    break;

                case BasePlayerController.PlayerState.Walk:
                    break;

                case BasePlayerController.PlayerState.Run:
                    break;

                case BasePlayerController.PlayerState.Crouch:
                    break;

                case BasePlayerController.PlayerState.Slide:
                    {
                        m_handIKActive = false;
                    }
                    break;

                case BasePlayerController.PlayerState.Falling:
                    break;
            }
        }

        private void HandlePlayerStatePopped(BasePlayerController.PlayerState poppedState, BasePlayerController.PlayerState nextTopState)
        {
            switch (poppedState)
            {
                case BasePlayerController.PlayerState.Idle:
                    break;

                case BasePlayerController.PlayerState.Walk:
                    break;

                case BasePlayerController.PlayerState.Run:
                    break;

                case BasePlayerController.PlayerState.Crouch:
                    break;

                case BasePlayerController.PlayerState.Slide:
                    {
                        if (m_currentWeaponController != null)
                        {
                            m_handIKActive = true;
                        }
                    }
                    break;

                case BasePlayerController.PlayerState.Falling:
                    break;
            }
        }

        #endregion Player State

        #region IK Sets

        private void HandleWeaponPickup(WeaponController weaponController, WeaponType weaponType)
        {
            m_currentWeaponController = weaponController;
            m_handIKActive = true;
        }

        private void ClearHandIK(WeaponController weaponController, WeaponType weaponType) => m_handIKActive = false;

        #endregion IK Sets
    }
}