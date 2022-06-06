using UnityEngine;

namespace Weapons
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private WeaponData m_weaponData;
        [SerializeField] private string m_weaponDefaultMask;
        [SerializeField] private string m_weaponDroppedMask;
        [SerializeField] private Rigidbody m_weaponRb;
        [SerializeField] private BoxCollider m_weaponCollider;

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
            gameObject.layer = LayerMask.NameToLayer(m_weaponDroppedMask);
        }
    }
}