#region

using System;
using UnityEngine;

#endregion

namespace Utils.Common
{
    public class Rotator : MonoBehaviour
    {
        [Header("Rotation Data")]
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private RotationAxis _rotationAxis;
        [SerializeField] private bool _rotateOnStart;

        private bool _isRotating;
        private RotationAxis _currentRotationAxis;

        #region Unity Functions

        private void Start() => _currentRotationAxis = _rotationAxis;

        private void Update()
        {
            if (_isRotating)
            {
                var rotation = new Vector3();
                switch (_rotationAxis)
                {
                    case RotationAxis.XAxis:
                        rotation.x += _rotationSpeed;
                        break;

                    case RotationAxis.YAxis:
                        rotation.y += _rotationSpeed;
                        break;

                    case RotationAxis.ZAxis:
                        rotation.z += _rotationSpeed;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                transform.Rotate(rotation * Time.deltaTime);
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void StartRotation() => _isRotating = true;

        public void StopRotation() => _isRotating = false;

        public void SetRotationAxis(RotationAxis rotationAxis) => _currentRotationAxis = rotationAxis;

        #endregion External Functions

        #region Enums

        public enum RotationAxis
        {
            XAxis,
            YAxis,
            ZAxis,
        }

        #endregion Enums
    }
}