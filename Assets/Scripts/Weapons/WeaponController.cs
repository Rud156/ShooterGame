using Common;
using UnityEngine;

namespace Weapons
{
    public class WeaponController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private WeaponData m_weaponData;
        [SerializeField] private WeaponRecoilData m_weaponRecoilData;
        [SerializeField] private string m_weaponDefaultMask;
        [SerializeField] private string m_weaponDroppedMask;

        [Header("Collision/RB")]
        [SerializeField] private Rigidbody m_weaponRb;
        [SerializeField] private BoxCollider m_weaponCollider;

        [Header("Hold Positions")]
        [SerializeField] private Transform m_weaponFrontPoint;
        [SerializeField] private Transform m_weaponTriggerPoint;

        [Header("Components")]
        [SerializeField] private DissolveShader m_weaponDissolveShader;

        private int m_bulletsShot;
        private float m_currentRecoilResetTime;
        private float m_lastShotTime;
        private float m_lastShotRemainderTime;

        #region Unity Functions

        private void Start()
        {
            m_lastShotTime = 0;
            m_lastShotRemainderTime = 0;
            m_bulletsShot = 0;
        }

        private void FixedUpdate()
        {
            if (m_currentRecoilResetTime >= 0)
            {
                m_currentRecoilResetTime -= Time.fixedDeltaTime;
                if (m_currentRecoilResetTime <= 0)
                {
                    m_lastShotRemainderTime = 0;
                    m_lastShotTime = 0;
                }
            }
        }

        #endregion

        #region Recoil Control

        private int CalculateShootCountAndSaveRemainder()
        {
            if (m_lastShotTime <= 0)
            {
                return 1;
            }

            float currentTime = Time.time;
            float diff = (currentTime - m_lastShotTime) + m_lastShotRemainderTime;
            int shootCount = Mathf.FloorToInt(diff / m_weaponRecoilData.fireRate);

            if (shootCount > 0)
            {
                float remainder = diff - shootCount * m_weaponRecoilData.fireRate;
                m_lastShotRemainderTime = remainder;
            }

            return shootCount;
        }

        private void ShootingSetComplete()
        {
            m_currentRecoilResetTime = m_weaponRecoilData.recoilResetDelay;
            m_lastShotTime = Time.time;
        }

        private void ResetRecoilData(int bulletsShot)
        {
            if (m_bulletsShot < 0)
            {
                m_bulletsShot = 0;
            }
            else
            {
                m_bulletsShot = bulletsShot;
            }
        }

        #endregion

        #region Weapon Data

        public WeaponData GetWeaponData() => m_weaponData;

        public void SetupWeaponDefaultsOnPickup()
        {
            m_weaponRb.isKinematic = true;
            m_weaponCollider.enabled = false;
            gameObject.layer = LayerMask.NameToLayer(m_weaponDefaultMask);
        }

        public void SetupWeaponDefaultsOnDrop()
        {
            m_weaponRb.isKinematic = false;
            m_weaponCollider.enabled = true;
            transform.localScale = m_weaponData.WeaponDroppedScale;
            gameObject.layer = LayerMask.NameToLayer(m_weaponDroppedMask);
        }

        public Transform GetFrontPoint() => m_weaponFrontPoint;

        public Transform GetTriggerPoint() => m_weaponTriggerPoint;

        #endregion

        #region Shader

        public void SetWeaponDissolve(bool active) => m_weaponDissolveShader.SetDissolve(active);

        #endregion
    }
}