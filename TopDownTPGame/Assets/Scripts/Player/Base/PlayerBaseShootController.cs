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

        private Transform _mainCamera;

        #region Unity Functions

        private void Start() => _mainCamera = Camera.main.transform;

        #endregion Unity Functions

        #region External Functions

        public Vector3 GetShootLookDirection()
        {
            var closeHit = Physics.Linecast(_shootPoint.position, _closeShootClearPoint.position, _shootMask);
            Debug.DrawLine(_shootPoint.position, _closeShootClearPoint.position, Color.red, 10);
            if (closeHit)
            {
                Debug.Log("Close Point Hit");
                return _cinemachineFollowPoint.forward.normalized;
            }

            var hit = Physics.Raycast(_mainCamera.position, _mainCamera.forward, out var hitInfo, _maxShootDistance, _shootMask);
            Debug.DrawRay(_mainCamera.position, _mainCamera.forward * _maxShootDistance, Color.red, 10);
            if (hit)
            {
                var direction = hitInfo.point - _shootPoint.position;
                Debug.Log("Hit Far Valid");
                return direction.normalized;
            }
            else
            {
                var distantPoint = _mainCamera.position + _mainCamera.forward.normalized * _maxShootDistance;
                var direction = distantPoint - _shootPoint.position;
                Debug.Log("Hit Far Invalid");
                return direction.normalized;
            }
        }

        public Vector3 GetShootPosition() => _shootPoint.position;

        public Transform GetShootPoint() => _shootPoint;

        #endregion External Functions
    }
}