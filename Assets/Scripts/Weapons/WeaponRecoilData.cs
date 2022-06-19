using UnityEngine;

namespace Weapons
{
    [CreateAssetMenu(fileName = "WeaponRecoilData", menuName = "ScriptableObjects/Weapons/RecoilData", order = 1)]
    public class WeaponRecoilData : ScriptableObject
    {
        [Header("Common")]
        public float fireRate;
        public float recoilResetDelay;
        public AnimationCurve recoilResetLerpSpeed;
        public float recoilShootLerpSpeed;
        public AnimationCurve recoilLerpCurve;
        public float verticalRecoilOffset = 10;
        public float horizontalRecoilOffset = 10;

        [Header("Normal Recoil")]
        public Vector2 defaultFiringError;
        public Vector2 movementFiringError;
        public int horizontalRecoilStartBullet;
        public float horizontalSinAmplitude;
        public float horizontalBulletSinMultiplier;
        public float hVOffsetAmount;
        public AnimationCurve raycastOffsetMultiplierX;
        public AnimationCurve raycastOffsetMultiplierY;
        public AnimationCurve crosshairMultiplierX;
        public AnimationCurve crosshairMultiplierY;
    }
}