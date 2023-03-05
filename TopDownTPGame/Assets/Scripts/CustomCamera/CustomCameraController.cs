﻿#region

using Cinemachine;
using UnityEngine;

#endregion

namespace CustomCamera
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class CustomCameraController : MonoBehaviour
    {
        private CinemachineVirtualCamera _cinemachine;
        private CinemachineBasicMultiChannelPerlin _cinemachineNoise;

        private bool _isShaking;

        private float _shakeTimeLeft;
        private float _frequency;
        private float _amplitude;

        #region Unity Scripts

        private void Start()
        {
            _cinemachine = GetComponent<CinemachineVirtualCamera>();
            _cinemachineNoise = _cinemachine.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        private void Update()
        {
            if (!_isShaking)
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
        }

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
    }
}