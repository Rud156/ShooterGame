#region

using UnityEngine;

#endregion

namespace Player.Base
{
    public class BaseShootController : MonoBehaviour
    {
        [Header("Shoot Data")]
        [SerializeField] private float _maxShootDistance = 250;
        [SerializeField] private LayerMask _shootMask;

        [Header("Components")]
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Transform _cameraMainPoint;
        [SerializeField] private Transform _closeShootClearPoint;
        [SerializeField] private Transform _cameraHolder;

        #region External Functions

        public Vector3 GetShootLookDirection()
        {
            var closeHit = Physics.Linecast(_shootPoint.position, _closeShootClearPoint.position, _shootMask);
            if (closeHit)
            {
                return _cameraHolder.forward.normalized;
            }

            var hit = Physics.Raycast(_cameraMainPoint.position, _cameraMainPoint.forward, out var hitInfo, _maxShootDistance, _shootMask);
            if (hit)
            {
                var direction = hitInfo.point - _shootPoint.position;
                return direction.normalized;
            }
            else
            {
                var distantPoint = _cameraMainPoint.position + _cameraMainPoint.forward.normalized * _maxShootDistance;
                var direction = distantPoint - _shootPoint.position;
                return direction.normalized;
            }
        }

        public Vector3 GetShootPosition() => _shootPoint.position;

        public Transform GetShootPoint() => _shootPoint;

        #endregion External Functions
    }
}