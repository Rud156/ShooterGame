using Misc;
using UnityEngine;

namespace Player
{
    public class PlayerInterractionCollider : MonoBehaviour
    {
        private GameObject m_weaponInContact;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(TagManager.WeaponTag))
            {
                m_weaponInContact = other.gameObject;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(TagManager.WeaponTag))
            {
                m_weaponInContact = null;
            }
        }

        public GameObject GetWeaponInContact() => m_weaponInContact;
    }
}