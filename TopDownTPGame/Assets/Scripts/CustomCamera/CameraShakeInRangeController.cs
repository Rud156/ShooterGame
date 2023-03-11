#region

using UnityEngine;
using Utils.Misc;

#endregion

namespace CustomCamera
{
    public class CameraShakeInRangeController : MonoBehaviour
    {
        private Camera _mainCamera;
        private CameraShakerInRange _cameraShakerInRange;

        private float _shakeDuration;

        #region Unity Functions

        private void Start() => _mainCamera = Camera.main;

        private void Update()
        {
            _shakeDuration -= Time.deltaTime;
            if (_shakeDuration <= 0)
            {
                Destroy(gameObject);
                return;
            }

            var cameraDistance = Vector3.Distance(_mainCamera.transform.position, transform.position);
            float mappedShakeFrequency = 0;
            float mappedShakeAmplitude = 0;

            if (cameraDistance <= _cameraShakerInRange.range)
            {
                mappedShakeFrequency = ExtensionFunctions.Map(cameraDistance, 0, _cameraShakerInRange.range, _cameraShakerInRange.maxFrequency, _cameraShakerInRange.minFrequency);
                mappedShakeAmplitude = ExtensionFunctions.Map(cameraDistance, 0, _cameraShakerInRange.range, _cameraShakerInRange.maxAmplitude, _cameraShakerInRange.minAmplitude);
            }

            CustomCameraController.Instance.UpdateFrequency(mappedShakeFrequency);
            CustomCameraController.Instance.UpdateAmplitude(mappedShakeAmplitude);
        }

        #endregion Unity Functions

        #region External Functions

        public void StartShake(CameraShakerInRange cameraShakerInRange)
        {
            _shakeDuration = cameraShakerInRange.duration;
            _cameraShakerInRange = cameraShakerInRange;
        }

        #endregion External Functions
    }
}