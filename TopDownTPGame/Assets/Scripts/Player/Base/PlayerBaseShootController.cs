#region

using UnityEngine;

#endregion

namespace Player.Base
{
    public class PlayerBaseShootController : MonoBehaviour
    {
        [Header("Shoot Data")]
        [SerializeField] private float _maxShootDistance = 50;
        [SerializeField] private LayerMask _shootMask;

        [Header("Components")]
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Transform _closeShootClearPoint;
        [SerializeField] private Transform _cinemachineFollowPoint;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private Transform _mainCamera;

        #region Unity Functions

        private void Start() => _mainCamera = Camera.main.transform;

        #endregion Unity Functions

        #region External Functions

        public Vector3 GetShootLookDirection()
        {
            var closeHit = Physics.Linecast(_shootPoint.position, _closeShootClearPoint.position, out var closeHitInfo, _shootMask);
            if (_debugIsActive)
            {
                var direction = _closeShootClearPoint.position - _shootPoint.position;
                var distance = Vector3.Distance(_shootPoint.position, _closeShootClearPoint.position);
                Debug.DrawRay(_shootPoint.position, direction * distance, Color.red, _debugDisplayDuration);
            }

            if (closeHit)
            {
                return _cinemachineFollowPoint.forward.normalized;
            }

            var hit = Physics.Raycast(_mainCamera.position, _mainCamera.forward, out var hitInfo, _maxShootDistance, _shootMask);
            if (_debugIsActive)
            {
                Debug.DrawRay(_mainCamera.position, _mainCamera.forward * _maxShootDistance, Color.red, _debugDisplayDuration);
            }

            if (hit)
            {
                var direction = hitInfo.point - _shootPoint.position;
                return direction.normalized;
            }
            else
            {
                var distantPoint = _mainCamera.position + _mainCamera.forward.normalized * _maxShootDistance;
                var direction = distantPoint - _shootPoint.position;
                return direction.normalized;
            }
        }

        public Vector3 GetShootPosition() => _shootPoint.position;

        public Transform GetShootPoint() => _shootPoint;

        #endregion External Functions
    }
}