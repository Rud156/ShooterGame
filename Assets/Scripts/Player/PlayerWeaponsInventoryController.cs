using UnityEngine;
using Utils.Input;
using Weapons;

namespace Player
{
    public class PlayerWeaponsInventoryController : MonoBehaviour
    {
        [Header("Weapon Pickup Raycast")]
        [SerializeField] private float m_weaponPickupRaycastDistance;
        [SerializeField] private LayerMask m_weaponPickupMask;

        [Header("Player Attach Points")]
        [SerializeField] private Transform m_leftHandAttachPoint;
        [SerializeField] private Transform m_rightHandAttachPoint;

        [Header("Components")]
        [SerializeField] private BasePlayerController m_playerController;
        [SerializeField] private PlayerInterractionCollider m_playerInterractionCollider;
        [SerializeField] private Transform m_playerWeaponAttachPoint;

        private WeaponType m_currentActiveWeapon;
        private WeaponController m_primaryWeapon;
        private WeaponController m_secondaryWeapon;

        public delegate void WeaponPickup(WeaponController weaponController);
        public delegate void WeaponDrop(WeaponController weaponController);

        public WeaponPickup OnWeaponPickup;
        public WeaponDrop OnWeaponDrop;

        #region Unity Functions

        private void Start()
        {
            m_currentActiveWeapon = WeaponType.Melee;
            m_playerController.OnPlayerStatePushed += HandlePlayerStatePushed;
            m_playerController.OnPlayerStatePopped += HandlePlayerStatePopped;
        }

        private void OnDestroy()
        {
            m_playerController.OnPlayerStatePushed -= HandlePlayerStatePushed;
            m_playerController.OnPlayerStatePopped -= HandlePlayerStatePopped;
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

            weapon.transform.SetParent(m_playerWeaponAttachPoint);
            m_primaryWeapon.SetupWeaponDefaultsOnPickup();

            WeaponData weaponData = weaponController.GetWeaponData();
            weapon.transform.localPosition = weaponData.WeaponTPLocalPosition;
            weapon.transform.localRotation = Quaternion.Euler(weaponData.WeaponTPLocalRotation);
            weapon.transform.localScale = weaponData.WeaponTPScale;

            SetActiveWeapon(WeaponType.Primary);
            OnWeaponPickup?.Invoke(weaponController);
        }

        public void DropCurrentPrimaryWeapon()
        {
            if (m_primaryWeapon != null)
            {
                m_primaryWeapon.transform.parent = null;
                m_primaryWeapon.SetupWeaponDefaultsOnDrop();
                OnWeaponDrop?.Invoke(m_primaryWeapon);
                m_primaryWeapon = null;
            }
            SetActiveWeapon(WeaponType.Melee);
        }

        public WeaponController GetPrimaryWeapon() => m_primaryWeapon;

        public WeaponController GetSecondaryWeapon() => m_secondaryWeapon;

        public WeaponController GetActiveWeapon() => m_primaryWeapon; // TODO: Implement active weapon

        #endregion Weapon Set

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
                        if (m_primaryWeapon != null)
                        {
                            m_primaryWeapon.SetWeaponDissolve(true);
                        }
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
                        if (m_primaryWeapon != null)
                        {
                            m_primaryWeapon.SetWeaponDissolve(false);
                        }
                    }
                    break;

                case BasePlayerController.PlayerState.Falling:
                    break;
            }
        }

        #endregion Player State
    }
}