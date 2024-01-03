using Cinemachine;
using UnityEngine;

namespace CustomCamera
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class CustomCameraController : MonoBehaviour
    {
        [Header("Camera Data")]
        [SerializeField] private GameObject _cameraShakeInRangePrefab;

        private CinemachineVirtualCamera _cinemachine;
        private CinemachineBasicMultiChannelPerlin _cinemachineNoise;

        private bool _isShaking;
        private CameraShakeType _cameraShakeType;
        public bool IsShaking => _isShaking;
        public CameraShakeType CustomCameraShakeType => _cameraShakeType;

        private float _shakeTimeLeft;
        private float _frequency;
        private float _amplitude;

        public delegate void CameraShakeStarted();
        public delegate void CameraShakeEnded();
        public CameraShakeStarted OnCameraShakeStarted;
        public CameraShakeEnded OnCameraShakeEnded;

        #region Unity Scripts

        private void Start()
        {
            _cinemachine = GetComponent<CinemachineVirtualCamera>();
            _cinemachineNoise = _cinemachine.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            _cameraShakeType = CameraShakeType.None;
        }

        private void Update()
        {
            if (!_isShaking || _cameraShakeType == CameraShakeType.Permament)
            {
                return;
            }

            _shakeTimeLeft -= Time.deltaTime;
            _cinemachineNoise.m_FrequencyGain = _frequency;
            _cinemachineNoise.m_AmplitudeGain = _amplitude;

            if (_shakeTimeLeft <= 0)
            {
                _cinemachineNoise.m_FrequencyGain = 0;
                _cinemachineNoise.m_AmplitudeGain = 0;
                _isShaking = false;
                _cameraShakeType = CameraShakeType.None;
                OnCameraShakeEnded?.Invoke();
            }
        }

        #endregion Unity Scripts

        #region External Functions

        public void StartShake(CameraShaker cameraShaker)
        {
            _frequency = cameraShaker.frequency;
            _amplitude = cameraShaker.amplitude;
            _shakeTimeLeft = cameraShaker.duration;
            _isShaking = true;
            _cameraShakeType = CameraShakeType.Temporary;
            OnCameraShakeStarted?.Invoke();
        }

        public void StartShake(CameraShakerInRange cameraShakerInRange, Vector3 spawnPosition)
        {
            _shakeTimeLeft = cameraShakerInRange.duration;
            _isShaking = true;
            _cameraShakeType = CameraShakeType.Temporary;
            OnCameraShakeStarted?.Invoke();

            var cameraShakerInRangeObject = Instantiate(_cameraShakeInRangePrefab, spawnPosition, Quaternion.identity);
            cameraShakerInRangeObject.GetComponent<CameraShakeInRangeController>().StartShake(cameraShakerInRange);
        }

        public void StartPermanentShake(CameraShaker cameraShaker)
        {
            _cinemachineNoise.m_FrequencyGain = cameraShaker.frequency;
            _cinemachineNoise.m_AmplitudeGain = cameraShaker.amplitude;
            _isShaking = true;
            _cameraShakeType = CameraShakeType.Permament;
            OnCameraShakeStarted?.Invoke();
        }

        public void EndPermanentShake()
        {
            _cinemachineNoise.m_FrequencyGain = 0;
            _cinemachineNoise.m_AmplitudeGain = 0;
            _isShaking = false;
            _cameraShakeType = CameraShakeType.None;
            OnCameraShakeEnded?.Invoke();
        }

        public void UpdateAmplitude(float changedAmplitude) => _amplitude = changedAmplitude;

        public void UpdateFrequency(float changedFrequency) => _frequency = changedFrequency;

        #endregion External Functions

        #region Singleton

        public static CustomCameraController Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        #endregion Singleton

        #region Enums

        public enum CameraShakeType
        {
            None,
            Temporary,
            Permament
        }

        #endregion Enums
    }
}