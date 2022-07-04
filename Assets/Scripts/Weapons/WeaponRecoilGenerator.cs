using UnityEngine;
using Random = UnityEngine.Random;

namespace Weapons
{
    public class WeaponRecoilGenerator : MonoBehaviour
    {
        #region Recoil

        public RecoilOffset CalculateRecoilData(WeaponRecoilData recoilData, WeaponRecoilData adsRecoilData, RecoilInputData recoilInputData)
        {
            Vector2 defaultFiringError = recoilData.defaultFiringError;
            Vector2 movementFiringError = recoilData.movementFiringError;
            int horizontalRecoilStartBullet = recoilData.horizontalRecoilStartBullet;
            float hVOffsetAmount = recoilData.hVOffsetAmount;
            float horizontalBulletSinMultiplier = recoilData.horizontalBulletSinMultiplier;
            float horizontalSinAmplitude = recoilData.horizontalSinAmplitude;
            AnimationCurve crossHairMultiplierX = recoilData.crosshairMultiplierX;
            AnimationCurve crossHairMultiplierY = recoilData.crosshairMultiplierY;
            AnimationCurve raycastOffsetMultiplierX = recoilData.raycastOffsetMultiplierX;
            AnimationCurve raycastOffsetMultiplierY = recoilData.raycastOffsetMultiplierY;

            if (recoilInputData.isInAds)
            {
                Debug.Log("Using Ads Data");
                defaultFiringError = adsRecoilData.defaultFiringError;
                movementFiringError = adsRecoilData.movementFiringError;
                horizontalRecoilStartBullet = adsRecoilData.horizontalRecoilStartBullet;
                hVOffsetAmount = adsRecoilData.hVOffsetAmount;
                horizontalBulletSinMultiplier = adsRecoilData.horizontalBulletSinMultiplier;
                horizontalSinAmplitude = adsRecoilData.horizontalSinAmplitude;
                crossHairMultiplierX = adsRecoilData.crosshairMultiplierX;
                crossHairMultiplierY = adsRecoilData.crosshairMultiplierY;
                raycastOffsetMultiplierX = adsRecoilData.raycastOffsetMultiplierX;
                raycastOffsetMultiplierY = adsRecoilData.raycastOffsetMultiplierY;
            }

            // Default/Movement Firing Error
            Vector2 firingError = recoilInputData.isMoving ? movementFiringError : defaultFiringError;
            Vector2 randomPointInCircle = Random.insideUnitCircle;
            Vector2 raycastOffset = new Vector2(randomPointInCircle.x * Random.Range(-firingError.x, firingError.x),
                                                Mathf.Abs(randomPointInCircle.y) * Random.Range(0, firingError.y));
            Vector2 crosshairOffset = Vector2.zero;

            // Calculate Horizontal Recoil
            bool cameraHorizontalMultNegative = false;
            if (recoilInputData.bulletsShot >= horizontalRecoilStartBullet)
            {
                raycastOffset.y = firingError.y + Random.Range(-hVOffsetAmount, hVOffsetAmount);
                raycastOffset.x = Mathf.Abs(raycastOffset.x);
                raycastOffset.x += recoilData.horizontalRecoilOffset;

                float sinBulletAngle = recoilInputData.bulletsShot - horizontalRecoilStartBullet;
                sinBulletAngle *= horizontalBulletSinMultiplier;
                sinBulletAngle %= 360;
                float sinAngle = Mathf.Deg2Rad * sinBulletAngle;

                raycastOffset.x *= Mathf.Sin(sinAngle) * horizontalSinAmplitude;
                if (sinBulletAngle > 90 && sinBulletAngle < 270)
                {
                    cameraHorizontalMultNegative = true;
                }
            }

            // Always add Vertical Recoil. It is ultimately multiplied by the Curve to obtain final value
            raycastOffset.y += recoilData.verticalRecoilOffset;
            crosshairOffset.y += recoilData.verticalRecoilOffset;

            // Always add Horizontal Recoil. It is ultimately multiplied by the Curve to obtain final value
            crosshairOffset.x += recoilData.horizontalRecoilOffset;

            // Calculate Camera Offset from Recoil
            crosshairOffset.x = Mathf.Abs(crosshairOffset.x);
            if (cameraHorizontalMultNegative)
            {
                crosshairOffset.x *= -1;
            }
            crosshairOffset.y = -Mathf.Abs(crosshairOffset.y);
            crosshairOffset.x *= crossHairMultiplierX.Evaluate(recoilInputData.bulletsShot);
            crosshairOffset.y *= crossHairMultiplierY.Evaluate(recoilInputData.bulletsShot);

            // Calculate offset from Crosshair
            raycastOffset.x *= raycastOffsetMultiplierX.Evaluate(recoilInputData.bulletsShot);
            raycastOffset.y *= raycastOffsetMultiplierY.Evaluate(recoilInputData.bulletsShot);

            return new RecoilOffset() { raycastOffset = raycastOffset, crosshairOffset = crosshairOffset };
        }

        #endregion Recoil

        #region Singleton

        private static WeaponRecoilGenerator _instance;
        public static WeaponRecoilGenerator Instance => _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        #endregion Singleton

        #region Structs

        public struct RecoilOffset
        {
            public Vector2 crosshairOffset;
            public Vector2 raycastOffset;
        }

        public struct RecoilInputData
        {
            public bool isInAds;
            public bool isMoving;
            public int bulletsShot;
        }

        #endregion Structs
    }
}