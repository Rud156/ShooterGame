using UnityEngine;

namespace Weapons
{
    public class WeaponAttacher : MonoBehaviour
    {
        [SerializeField] private Transform m_leftHandAttachPoint;
        [SerializeField] private Transform m_rightHandAttachPoint;

        private Transform m_weaponTransform;

        #region Unity Functions

        private void OnAnimatorIK(int layerIndex)
        {
        }

        #endregion Unity Functions

        #region Weapon Set

        public void SetPlayerWeapon(Transform weapon)
        {
            m_weaponTransform = weapon;
            // TODO: Turn off Weapon Gravity
        }

        public void DropCurrentWeapon()
        {
            m_weaponTransform = null;
            // TODO: Call something on the weapon to make it drop to the ground
        }

        #endregion Weapon Set
    }
}