#region

using System;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

namespace Player.Type_4
{
    public class Type_4_DroneController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform _droneTransform;

        [Header("Hover")]
        [SerializeField] private float _hoverMinDistance;
        [SerializeField] private float _hoverMaxDistance;
        [SerializeField] private Vector3 _hoverRandomDistance;
        [SerializeField] private float _hoverMinSpeed;
        [SerializeField] private float _hoverMaxSpeed;

        [Header("Drone Shoot Knockback")]
        [SerializeField] private float _knockbackDistance;
        [SerializeField] private Vector3 _knockbackRandomDistance;
        [SerializeField] private float _knockbackLerpSpeed;
        [SerializeField] private float _knockbackResetLerpSpeed;

        private DroneState _droneState;

        private float _currentLerpSpeed;
        private bool _currentStatePositive;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _lerpAmount;

        #region Unity Functions

        private void Start()
        {
            _lerpAmount = 1;
            SetDroneState(DroneState.HoverIdle);
        }

        private void FixedUpdate()
        {
            if (_lerpAmount >= 1)
            {
                switch (_droneState)
                {
                    case DroneState.HoverIdle:
                        UpdateHoverIdle();
                        break;

                    case DroneState.Knockback:
                        UpdateKnockback();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            UpdateLerp();
        }

        #endregion Unity Functions

        #region External Functions

        public void KnockbackDrone(float knockbackMultiplier)
        {
            var dronePosition = _droneTransform.localPosition;
            var targetPosition = new Vector3(dronePosition.x, dronePosition.y, _knockbackDistance);

            var randomX = Random.Range(-_knockbackRandomDistance.x, _knockbackRandomDistance.x);
            var randomY = Random.Range(-_knockbackRandomDistance.y, _knockbackRandomDistance.y);
            var randomZ = Random.Range(-_knockbackRandomDistance.z, _knockbackRandomDistance.z);
            targetPosition.x += randomX;
            targetPosition.y += randomY;
            targetPosition.z += randomZ;

            _targetPosition = targetPosition * knockbackMultiplier;
            _startPosition = dronePosition;
            _lerpAmount = 0;
            _currentLerpSpeed = _knockbackLerpSpeed;
            _currentStatePositive = true;

            SetDroneState(DroneState.Knockback);
        }

        #endregion External Functions

        #region Utils

        private void UpdateHoverIdle()
        {
            // _currentStatePositive -> True: Drone Up, False: Drone Down
            _targetPosition = _currentStatePositive ? new Vector3(0, _hoverMinDistance, 0) : new Vector3(0, _hoverMaxDistance, 0);
            _targetPosition.x += Random.Range(-_hoverRandomDistance.x, _hoverRandomDistance.x);
            _targetPosition.y += Random.Range(-_hoverRandomDistance.y, _hoverRandomDistance.y);
            _targetPosition.z += Random.Range(-_hoverRandomDistance.z, _hoverRandomDistance.z);

            _lerpAmount = 0;
            _startPosition = _droneTransform.localPosition;
            _currentLerpSpeed = Random.Range(_hoverMinSpeed, _hoverMaxSpeed);
            _currentStatePositive = !_currentStatePositive;
        }

        private void UpdateKnockback()
        {
            // _currentStatePositive -> True: Knocked Back, False: Resetting
            if (_currentStatePositive)
            {
                _targetPosition = Vector3.zero;
                _currentLerpSpeed = _knockbackResetLerpSpeed;
                SetDroneState(DroneState.HoverIdle);
            }

            _lerpAmount = 0;
            _startPosition = _droneTransform.localPosition;
            _currentStatePositive = !_currentStatePositive;
        }

        private void UpdateLerp()
        {
            if (_lerpAmount >= 1)
            {
                return;
            }

            var lerpPosition = Vector3.Lerp(_startPosition, _targetPosition, _lerpAmount);
            _lerpAmount += _currentLerpSpeed * Time.fixedDeltaTime;
            _droneTransform.localPosition = lerpPosition;
        }

        private void SetDroneState(DroneState droneState) => _droneState = droneState;

        #endregion Utils

        #region Enums

        private enum DroneState
        {
            HoverIdle,
            Knockback,
        };

        #endregion Enums
    }
}