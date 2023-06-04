#region

using System;
using UnityEngine;
using UnityEngine.Assertions;

#endregion

namespace Player.Base
{
    public class PlayerBaseShootController : MonoBehaviour
    {
        [Header("Shoot Data")]
        [SerializeField] private float _virtualShootPointLocalZOffset;
        [SerializeField] private float _virtualShootPointWorldYOffset;
        [SerializeField] private float _maxShootDistance = 500;
        [SerializeField] private LayerMask _shootMask;

        [Header("Components")]
        [SerializeField] private Transform _virtualShootPoint;
        [SerializeField] private Transform _physicalShootPoint;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private Transform _mainCamera;

        #region Unity Functions

        private void Start() => _mainCamera = Camera.main!.transform;

        private void LateUpdate() => AdjustVirtualShootPointToHeight();

        #endregion Unity Functions

        #region External Functions

        public Vector3 GetShootLookDirection() => GetShootLookDirection(_shootMask);

        public Vector3 GetShootLookDirection(LayerMask customLayerMask)
        {
            var hit = Physics.Raycast(_virtualShootPoint.position, _virtualShootPoint.forward, out var hitInfo, _maxShootDistance, customLayerMask);
            if (_debugIsActive)
            {
                Debug.DrawRay(_virtualShootPoint.position, _virtualShootPoint.forward * _maxShootDistance, Color.red, _debugDisplayDuration);
            }

            if (hit)
            {
                var direction = hitInfo.point - _physicalShootPoint.position;
                return direction.normalized;
            }
            else
            {
                var distantPoint = _virtualShootPoint.position + _virtualShootPoint.forward.normalized * _maxShootDistance;
                var direction = distantPoint - _physicalShootPoint.position;
                return direction.normalized;
            }
        }

        public Vector3 GetShootPosition() => _physicalShootPoint.position;

        public Transform GetShootPoint() => _physicalShootPoint;

        #endregion External Functions

        #region Utils

        private void AdjustVirtualShootPointToHeight()
        {
            var localPosition = _virtualShootPoint.localPosition;
            localPosition.z = _virtualShootPointLocalZOffset;
            _virtualShootPoint.localPosition = localPosition;

            var worldPosition = _virtualShootPoint.position;
            worldPosition.y = _virtualShootPointWorldYOffset;
            _virtualShootPoint.position = worldPosition;
        }

        #endregion Utils
    }
}