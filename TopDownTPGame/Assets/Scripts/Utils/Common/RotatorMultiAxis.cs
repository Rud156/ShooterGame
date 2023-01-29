#region

using UnityEngine;

#endregion

namespace Utils.Common
{
    public class RotatorMultiAxis : MonoBehaviour
    {
        [Header("Rotation Data")]
        [SerializeField] private Vector3 _rotationSpeed;
        [SerializeField] private bool _rotateOnStart;

        private bool _isRotating;

        #region Unity Functions

        private void Start()
        {
            if (_rotateOnStart)
            {
                _isRotating = true;
            }
        }

        private void Update()
        {
            if (!_isRotating)
            {
                return;
            }

            transform.Rotate(_rotationSpeed * Time.deltaTime);
        }

        #endregion Unity Functions

        #region External Functions

        public void StartRotation() => _isRotating = true;

        public void StopRotation() => _isRotating = false;

        #endregion External Functions
    }
}