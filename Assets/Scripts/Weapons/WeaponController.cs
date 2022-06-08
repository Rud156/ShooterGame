using UnityEngine;

namespace Weapons
{
    public class WeaponController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private WeaponData m_weaponData;
        [SerializeField] private string m_weaponDefaultMask;
        [SerializeField] private string m_weaponDroppedMask;

        [Header("Collision/RB")]
        [SerializeField] private Rigidbody m_weaponRb;
        [SerializeField] private BoxCollider m_weaponCollider;

        [Header("Hold Positions")]
        [SerializeField] private Transform m_weaponFrontPoint;
        [SerializeField] private Transform m_weaponTriggerPoint;

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

        public Transform GetFrontPoint() => m_weaponFrontPoint;

        public Transform GetTriggerPoint() => m_weaponTriggerPoint;
    }
}