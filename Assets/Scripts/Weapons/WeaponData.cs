using UnityEngine;

namespace Weapons
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/Weapons/Data", order = 1)]
    public class WeaponData : ScriptableObject
    {
        public string WeaponName;
        public WeaponType weaponType;
        public GameObject Weapon;

        [Header("First Person View")]
        public Vector3 WeaponScale;
        public Vector3 WeaponLocalPosition;
        public Vector3 WeaponLocalRotation;

        [Header("Third Person View")]
        public Vector3 WeaponTPScale;
        public Vector3 WeaponTPLocalPosition;
        public Vector3 WeaponTPLocalRotation;

        [Header("Weapon Dropped")]
        public Vector3 WeaponDroppedScale;
    }
}