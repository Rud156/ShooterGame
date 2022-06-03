using UnityEngine;

namespace Weapons
{
    public class WeaponDataHolder : MonoBehaviour
    {
        [SerializeField] private WeaponData m_weaponData;

        public WeaponData GetWeaponData() => m_weaponData;
    }
}