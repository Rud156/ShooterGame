using UnityEngine;
using Utils.Input;
using Weapons;

namespace Player
{
    public class PlayerWeaponsController : MonoBehaviour
    {
        [Header("Weapon Pickup Raycast")]
        [SerializeField] private float m_weaponPickupRaycastDistance;
        [SerializeField] private LayerMask m_weaponPickupMask;

        [Header("Player Attach Points")]
        [SerializeField] private Transform m_leftHandAttachPoint;
        [SerializeField] private Transform m_rightHandAttachPoint;

        [Header("Components")]
        [SerializeField] private Transform m_pickupRaycastTransform;
        [SerializeField] private Transform m_playerCamera;

        private WeaponController m_primaryWeapon;
        private WeaponController m_secondaryWeapon;

        #region Unity Functions

        private void Update()
        {
            if (Input.GetKey(InputKeys.WeaponPickup) && m_primaryWeapon == null)
            {
                Vector3 forward = m_playerCamera.forward;
                Debug.DrawRay(m_pickupRaycastTransform.position, forward * m_weaponPickupRaycastDistance, Color.green);
                if (Physics.Raycast(m_pickupRaycastTransform.position, forward, out RaycastHit hit, m_weaponPickupRaycastDistance, m_weaponPickupMask))
                {
                    GameObject hitObject = hit.collider.gameObject;
                    WeaponController weaponController = hitObject.GetComponent<WeaponController>();
                    if (weaponController != null)
                    {
                        SetPlayerPrimaryWeapon(weaponController, hitObject);
                    }
                }
            }
            else if (Input.GetKeyDown(InputKeys.WeaponDrop))
            {
                DropCurrentWeapon();
            }
        }

        #endregion Unity Functions

        #region Weapon Set

        public void SetPlayerPrimaryWeapon(WeaponController weaponController, GameObject weapon)
        {
            m_primaryWeapon = weaponController;

            weapon.transform.SetParent(m_playerCamera);
            m_primaryWeapon.SetupWeaponDefaultsOnPickup();

            WeaponData weaponData = weaponController.GetWeaponData();
            weapon.transform.localRotation = Quaternion.Euler(weaponData.WeaponLocalRotation);
            weapon.transform.localScale = weaponData.WeaponScale;
            weapon.transform.localPosition = weaponData.WeaponLocalPosition;
        }

        public void DropCurrentWeapon()
        {
            if (m_primaryWeapon != null)
            {
                m_primaryWeapon.transform.parent = null;
                m_primaryWeapon.SetupWeaponDefaultsOnDrop();
                m_primaryWeapon = null;
            }
        }

        #endregion Weapon Set
    }
}