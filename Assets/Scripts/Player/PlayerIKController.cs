using UnityEngine;
using Weapons;

namespace Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerIKController : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponsController m_playerWeaponsController;

        private Animator m_animator;

        private bool m_handIKActive;
        private WeaponController m_currentWeaponController;

        #region Unity Functions

        private void Start()
        {
            m_playerWeaponsController.OnWeaponPickup += HandleWeaponPickup;
            m_playerWeaponsController.OnWeaponDrop += ClearHandIK;

            m_animator = GetComponent<Animator>();
        }

        private void OnDestroy()
        {
            m_playerWeaponsController.OnWeaponPickup -= HandleWeaponPickup;
            m_playerWeaponsController.OnWeaponDrop -= ClearHandIK;
        }

        private void OnAnimatorIK()
        {
            if (m_handIKActive)
            {
                Vector3 leftHandPosition = m_currentWeaponController.GetFrontPoint().position;
                Vector3 rightHandPosition = m_currentWeaponController.GetTriggerPoint().position;
                m_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                m_animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPosition);
                m_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                m_animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandPosition);
            }
            else
            {
                m_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                m_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            }
        }

        #endregion

        #region IK Sets

        private void HandleWeaponPickup(WeaponController weaponController)
        {
            m_currentWeaponController = weaponController;
            m_handIKActive = true;
        }

        private void ClearHandIK() => m_handIKActive = false;

        #endregion
    }
}