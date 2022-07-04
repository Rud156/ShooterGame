using UnityEngine;

namespace Weapons
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/Weapons/Data", order = 1)]
    public class WeaponData : ScriptableObject
    {
        public string WeaponName;
        public WeaponType weaponType;
        public GameObject Weapon;

        [Header("Third Person View")]
        public Vector3 WeaponTPLocalPosition;
        public Vector3 WeaponTPLocalRotation;
        public Vector3 WeaponTPScale;

        [Header("Weapon Dropped")]
        public Vector3 WeaponDroppedScale;

        [Header("Recoil")]
        public WeaponRecoilData NormalRecoilData;
        public WeaponRecoilData AdsRecoilData;
    }
}