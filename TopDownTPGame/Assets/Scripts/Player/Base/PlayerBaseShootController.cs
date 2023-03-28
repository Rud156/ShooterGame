#region

using UnityEngine;
using UnityEngine.Assertions;

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

        public Vector3 GetShootLookDirection(bool skipCloseCheck = false, bool useCustomShootPoint = false, Transform customShootPoint = null) =>
            GetShootLookDirection(_shootMask, skipCloseCheck, useCustomShootPoint, customShootPoint);

        public Vector3 GetShootLookDirection(LayerMask customLayerMask, bool skipCloseCheck = false, bool useCustomShootPoint = false, Transform customShootPoint = null)
        {
            var shootPoint = useCustomShootPoint ? customShootPoint : _shootPoint;
            Assert.IsNotNull(shootPoint, nameof(shootPoint) + " != null");
            var shootPointPosition = shootPoint.position;

            if (skipCloseCheck)
            {
                var closeHit = Physics.Linecast(shootPointPosition, _closeShootClearPoint.position, out _, customLayerMask);
                if (_debugIsActive)
                {
                    var clearShootPointPosition = _closeShootClearPoint.position;
                    var direction = clearShootPointPosition - shootPointPosition;
                    var distance = Vector3.Distance(shootPointPosition, clearShootPointPosition);
                    Debug.DrawRay(shootPointPosition, direction * distance, Color.red, _debugDisplayDuration);
                }

                if (closeHit)
                {
                    return _cinemachineFollowPoint.forward.normalized;
                }
            }

            var hit = Physics.Raycast(_mainCamera.position, _mainCamera.forward, out var hitInfo, _maxShootDistance, customLayerMask);
            if (_debugIsActive)
            {
                Debug.DrawRay(_mainCamera.position, _mainCamera.forward * _maxShootDistance, Color.red, _debugDisplayDuration);
            }

            if (hit)
            {
                var direction = hitInfo.point - shootPointPosition;
                return direction.normalized;
            }
            else
            {
                var distantPoint = _mainCamera.position + _mainCamera.forward.normalized * _maxShootDistance;
                var direction = distantPoint - shootPointPosition;
                return direction.normalized;
            }
        }

        public Vector3 GetShootPosition() => _shootPoint.position;

        public Transform GetShootPoint() => _shootPoint;

        #endregion External Functions
    }
}