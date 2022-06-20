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

        public float m_recoilLerpSpeed;
        public float m_recoilLerpAmount;
        public bool m_resetRecoil;
        public Vector2 m_startRecoilOffset;
        public Vector2 m_targetRecoilOffset;
        public Vector2 currentInputAmount;
        public Vector2 nextInputAmount;
        public Vector2 diff;

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
                if (Input.GetKey(InputKeys.AttackPrimary))
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

            Debug.Log($"Start: {m_startRecoilOffset}, Target: {m_targetRecoilOffset}");

            float currentRecoilAmount = recoilData.recoilLerpCurve.Evaluate(m_recoilLerpAmount);
            currentInputAmount = Vector2.Lerp(m_startRecoilOffset, m_targetRecoilOffset, currentRecoilAmount);
            Debug.Log($"Previous: CR: {currentRecoilAmount}, CI: {currentInputAmount}, RL: {m_recoilLerpAmount}");
            m_recoilLerpAmount += m_recoilLerpSpeed * Time.deltaTime;
            m_recoilLerpAmount = Mathf.Clamp01(m_recoilLerpAmount);

            float nextRecoilAmount = recoilData.recoilLerpCurve.Evaluate(m_recoilLerpAmount);
            nextInputAmount = Vector2.Lerp(m_startRecoilOffset, m_targetRecoilOffset, nextRecoilAmount);

            Debug.Log($"Next: RL: {m_recoilLerpAmount}, NR: {nextRecoilAmount}, NI: {nextInputAmount}");

            diff = nextInputAmount - currentInputAmount;

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
                Debug.Log($"Shoot Count Valid: {shootCount}");
                WeaponRecoilGenerator.RecoilOffset recoilOffset = WeaponRecoilGenerator.Instance.CalculateRecoilData(recoilData, new WeaponRecoilGenerator.RecoilInputData()
                {
                    bulletsShot = activeWeapon.GetCurrentBulletsShot(),
                    isInAds = false,
                    isMoving = false
                });
                activeWeapon.SetCurrentBulletsShot(activeWeapon.GetCurrentBulletsShot() + 1);

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

            DebugExtension.DebugWireSphere(sphereLocation, Color.red, 1, 1);
        }

        #endregion Shooting
    }
}