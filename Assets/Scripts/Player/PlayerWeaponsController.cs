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
        [SerializeField] private PlayerInterractionCollider m_playerInterractionCollider;
        [SerializeField] private Transform m_playerCamera;

        private WeaponType m_currentActiveWeapon;
        private WeaponController m_primaryWeapon;
        private WeaponController m_secondaryWeapon;

        public delegate void WeaponPickup(WeaponController weaponController);
        public delegate void WeaponDrop();

        public WeaponPickup OnWeaponPickup;
        public WeaponDrop OnWeaponDrop;

        #region Unity Functions

        private void Start()
        {
            m_currentActiveWeapon = WeaponType.Melee;
        }

        private void Update()
        {
            if (Input.GetKeyDown(InputKeys.WeaponPickup))
            {
                if (m_playerInterractionCollider.GetWeaponInContact() != null)
                {
                    GameObject hitObject = m_playerInterractionCollider.GetWeaponInContact();
                    WeaponController weaponController = hitObject.GetComponent<WeaponController>();
                    if (weaponController != null)
                    {
                        SetPlayerPrimaryWeapon(weaponController, hitObject);
                    }
                }
            }
            else if (Input.GetKeyDown(InputKeys.WeaponDrop))
            {
                DropCurrentPrimaryWeapon();
            }
        }

        #endregion Unity Functions

        #region Weapon Set

        private void SetActiveWeapon(WeaponType weaponType) => m_currentActiveWeapon = weaponType;

        public void SetPlayerPrimaryWeapon(WeaponController weaponController, GameObject weapon)
        {
            if (m_primaryWeapon != null)
            {
                DropCurrentPrimaryWeapon();
            }

            m_primaryWeapon = weaponController;

            weapon.transform.SetParent(m_playerCamera);
            m_primaryWeapon.SetupWeaponDefaultsOnPickup();

            WeaponData weaponData = weaponController.GetWeaponData();
            weapon.transform.localRotation = Quaternion.Euler(weaponData.WeaponLocalRotation);
            weapon.transform.localScale = weaponData.WeaponScale;
            weapon.transform.localPosition = weaponData.WeaponLocalPosition;

            // TODO: Handle this for Third Person View also
            SetActiveWeapon(WeaponType.Primary);
            OnWeaponPickup?.Invoke(weaponController);
        }

        public void DropCurrentPrimaryWeapon()
        {
            if (m_primaryWeapon != null)
            {
                m_primaryWeapon.transform.parent = null;
                m_primaryWeapon.SetupWeaponDefaultsOnDrop();
                m_primaryWeapon = null;
            }
            SetActiveWeapon(WeaponType.Melee);
            OnWeaponDrop?.Invoke();
        }

        #endregion Weapon Set
    }
}