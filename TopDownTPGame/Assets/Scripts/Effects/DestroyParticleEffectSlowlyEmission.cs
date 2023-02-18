#region

using System;
using UnityEngine;

#endregion

namespace Effects
{
    public class DestroyParticleEffectSlowlyEmission : MonoBehaviour
    {
        [Header("Destroy Data")]
        [SerializeField] private float _destroyAfterTime;

        [Header("Particle Value")]
        [SerializeField] private float _emissionChangeRate;
        [SerializeField] private float _startEmissionRate;
        [SerializeField] private float _endEmissionRate;

        private bool _isDestroyActive;
        private ParticleSystem _particleSystem;

        private DestroyState _destroyState;
        private float _floatValue;

        #region Unity Functions

        private void Update()
        {
            if (!_isDestroyActive)
            {
                return;
            }

            switch (_destroyState)
            {
                case DestroyState.EmissionState:
                {
                    var emission = _particleSystem.emission;

                    _floatValue += _emissionChangeRate * Time.deltaTime;
                    var mappedEmissionRate = Mathf.Lerp(_startEmissionRate, _endEmissionRate, _floatValue);
                    emission.rateOverTime = mappedEmissionRate;

                    if (_floatValue >= 1)
                    {
                        _floatValue = _destroyAfterTime;
                        _destroyState = DestroyState.DelayedDestroy;
                    }
                }
                    break;

                case DestroyState.DelayedDestroy:
                {
                    _floatValue -= Time.fixedDeltaTime;
                    if (_floatValue <= 0)
                    {
                        Destroy(gameObject);
                    }
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void DestroyEffect()
        {
            _isDestroyActive = true;
            _particleSystem = GetComponent<ParticleSystem>();
            _destroyState = DestroyState.EmissionState;
            _floatValue = 0;
        }

        #endregion External Functions

        #region Enums

        private enum DestroyState
        {
            EmissionState,
            DelayedDestroy,
        }

        #endregion Enums
    }
}