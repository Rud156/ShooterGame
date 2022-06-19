using UnityEngine;
using Utils.Input;
using Weapons;

namespace Player
{
    public class PlayerWeaponRecoilController : MonoBehaviour
    {
        [Header("Recoil Camera Data")]
        [SerializeField] private float m_recoilCameraMultiplier;

        [Header("Components")]
        [SerializeField] private PlayerWeaponsInventoryController m_playerWeaponInventory;
        [SerializeField] private Transform m_cameraHolder;
        [SerializeField] private Transform m_mainCamera;
        [SerializeField] private Transform m_weaponShootClearPoint;
        [SerializeField] private LayerMask m_weaponShootMask;

        private float m_recoilLerpSpeed;
        private float m_recoilLerpAmount;
        private bool m_resetRecoil;
        private Vector2 m_startRecoilOffset;
        private Vector2 m_targetRecoilOffset;

        #region Unity Functions

        private void Start()
        {
            m_playerWeaponInventory.OnWeaponPickup += HandleWeaponPickup;
            m_playerWeaponInventory.OnWeaponDrop += HandleWeaponDrop;

            ClearRecoilData();
        }

        private void OnDestroy()
        {
            m_playerWeaponInventory.OnWeaponPickup -= HandleWeaponPickup;
            m_playerWeaponInventory.OnWeaponDrop -= HandleWeaponDrop;
        }

        private void Update()
        {
            if (m_playerWeaponInventory.GetActiveWeapon() != null)
            {
                UpdateRecoilCamera();
                if (Input.GetKeyDown(InputKeys.AttackPrimary))
                {
                    HandleShootingPressed();
                }
            }
        }

        #endregion Unity Functions

        #region Weapon Pickup And Drop

        private void HandleWeaponPickup(WeaponController weaponController)
        {
            ClearRecoilData();
            weaponController.OnRecoilReset += ResetPreRecoilCamera;
        }

        private void HandleWeaponDrop(WeaponController weaponController)
        {
            ClearRecoilData();
            weaponController.OnRecoilReset -= ResetPreRecoilCamera;
        }

        #endregion Weapon Pickup And Drop

        #region Shooting

        private void UpdateRecoilCamera()
        {
            if (m_recoilLerpAmount >= 1)
            {
                return;
            }

            WeaponController activeWeapon = m_playerWeaponInventory.GetActiveWeapon();
            WeaponRecoilData recoilData = activeWeapon.GetWeaponRecoilData();
            if (m_resetRecoil)
            {
                m_recoilLerpSpeed = recoilData.recoilResetLerpSpeed.Evaluate(m_recoilLerpAmount);
            }

            float currentRecoilAmount = recoilData.recoilLerpCurve.Evaluate(m_recoilLerpAmount);
            Vector2 currentInputAmount = Vector2.Lerp(m_startRecoilOffset, m_targetRecoilOffset, currentRecoilAmount);
            m_recoilLerpAmount += m_recoilLerpSpeed * Time.fixedDeltaTime;
            m_recoilLerpAmount = Mathf.Clamp01(m_recoilLerpAmount);

            float nextRecoilAmount = recoilData.recoilLerpCurve.Evaluate(m_recoilLerpAmount);
            Vector2 nextInputAmount = Vector2.Lerp(m_startRecoilOffset, m_targetRecoilOffset, nextRecoilAmount);

            Vector2 diff = nextInputAmount - currentInputAmount;

            Vector3 rotation = transform.rotation.eulerAngles;
            rotation.y += diff.y * m_recoilCameraMultiplier;
            transform.rotation = Quaternion.Euler(rotation);

            Vector3 cameraHolderRotation = m_cameraHolder.rotation.eulerAngles;
            cameraHolderRotation.x += diff.x * m_recoilCameraMultiplier;
            m_cameraHolder.rotation = Quaternion.Euler(cameraHolderRotation);

            if (m_recoilLerpAmount >= 1)
            {
                m_startRecoilOffset = m_targetRecoilOffset;
                if (m_resetRecoil)
                {
                    ClearRecoilData();
                    activeWeapon.ResetRecoilData(0);
                }
            }
        }

        private void ResetPreRecoilCamera()
        {
            WeaponController activeWeapon = m_playerWeaponInventory.GetActiveWeapon();
            WeaponRecoilData recoilData = activeWeapon.GetWeaponRecoilData();

            float amount = recoilData.recoilLerpCurve.Evaluate(m_recoilLerpAmount);
            Vector2 currentRecoilAmount = Vector2.Lerp(m_startRecoilOffset, m_targetRecoilOffset, amount);
            m_startRecoilOffset = currentRecoilAmount;
            m_targetRecoilOffset = Vector2.zero;
            m_recoilLerpAmount = 0;
            m_resetRecoil = true;
            m_recoilLerpSpeed = recoilData.recoilResetLerpSpeed.Evaluate(0);
        }

        private void ClearRecoilData()
        {
            m_startRecoilOffset = Vector2.zero;
            m_targetRecoilOffset = Vector2.zero;
            m_recoilLerpAmount = 1;
            m_recoilLerpSpeed = 1;
            m_resetRecoil = false;
        }

        public void HandleShootingPressed()
        {
            WeaponController activeWeapon = m_playerWeaponInventory.GetActiveWeapon();
            int shootCount = activeWeapon.CalculateShootCountAndSaveRemainder();
            if (shootCount <= 0)
            {
                return;
            }

            WeaponRecoilData recoilData = activeWeapon.GetWeaponRecoilData();
            float recoilLerpAmount = recoilData.recoilLerpCurve.Evaluate(m_recoilLerpAmount);
            Vector2 currentRecoilAmount = Vector2.Lerp(m_startRecoilOffset, m_targetRecoilOffset, recoilLerpAmount);
            if (m_resetRecoil)
            {
                float recoilLerp = 1 - recoilLerpAmount;
                int bulletCount = (int)(activeWeapon.GetCurrentBulletsShot() * recoilLerp);
                activeWeapon.ResetRecoilData(bulletCount);

                m_targetRecoilOffset = currentRecoilAmount;
            }
            m_startRecoilOffset = currentRecoilAmount;

            for (int i = 0; i < shootCount; i++)
            {
                WeaponRecoilGenerator.RecoilOffset recoilOffset = WeaponRecoilGenerator.Instance.CalculateRecoilData(recoilData, new WeaponRecoilGenerator.RecoilInputData()
                {
                    bulletsShot = activeWeapon.GetCurrentBulletsShot(),
                    isInAds = false,
                    isMoving = false
                });

                m_targetRecoilOffset += recoilOffset.crosshairOffset;
                m_recoilLerpAmount = 0;
                m_recoilLerpSpeed = recoilData.recoilShootLerpSpeed;
                m_resetRecoil = false;

                Vector3 startPosition = m_cameraHolder.position;
                Vector3 endPosition = startPosition + m_mainCamera.forward * recoilData.maxShootDistance +
                                    m_mainCamera.up * recoilOffset.raycastOffset.y +
                                    m_mainCamera.right * recoilOffset.raycastOffset.x;
                BulletShot(startPosition, endPosition, activeWeapon.GetShootPoint().position);
            }
            activeWeapon.ShootingSetComplete();
        }

        private void BulletShot(Vector3 startPosition, Vector3 endPosition, Vector3 shootPoint)
        {
            Vector3 sphereLocation = endPosition;

            Vector3 wallCheckStartPosition = shootPoint;
            Vector3 wallCheckEndPosition = m_weaponShootClearPoint.position;
            bool wallHitCheck = Physics.Linecast(wallCheckStartPosition, wallCheckEndPosition, out RaycastHit hit, m_weaponShootMask);

            if (wallHitCheck)
            {
                Debug.DrawLine(wallCheckStartPosition, wallCheckEndPosition, Color.red, 1);
                sphereLocation = hit.point;
            }
            else
            {
                Debug.DrawLine(startPosition, endPosition, Color.red, 1);
                bool didHit = Physics.Linecast(startPosition, endPosition, out RaycastHit shootHit, m_weaponShootMask);
                if (didHit)
                {
                    sphereLocation = shootHit.point;
                }
            }

            DebugExtension.DebugWireSphere(sphereLocation, Color.red, 5, 1);
        }

        #endregion Shooting
    }
}