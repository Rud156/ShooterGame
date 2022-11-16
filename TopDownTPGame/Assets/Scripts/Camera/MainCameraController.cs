using UnityEngine;
using Utils.Input;
using Utils.Misc;

namespace Camera
{
    public class MainCameraController : MonoBehaviour
    {
        [SerializeField] private Transform _cameraHolder;
        [SerializeField] private float _cameraRotationSpeed;
        [SerializeField] private float _minCameraAngle;
        [SerializeField] private float _maxCameraAngle;

        private Vector2 _mouseInput;

        #region Unity Functions

        private void Start() => _mouseInput = new Vector2();

        private void Update() => HandleMouseInput();

        private void FixedUpdate() => UpdateCameraControl();

        #endregion Unity Functions

        #region Camera Control

        private void UpdateCameraControl()
        {
            Vector3 cameraRotation = _cameraHolder.rotation.eulerAngles;
            cameraRotation.y += _mouseInput.x * _cameraRotationSpeed * Time.fixedDeltaTime;
            cameraRotation.x += _mouseInput.y * _cameraRotationSpeed * Time.fixedDeltaTime;
            cameraRotation.x = ExtensionFunctions.To360Angle(cameraRotation.x);

            // Clamp X Rotation
            if (cameraRotation.x >= 0 && cameraRotation.x <= 180)
            {
                if (cameraRotation.x > _maxCameraAngle)
                {
                    cameraRotation.x = _maxCameraAngle;
                }
            }
            else if (cameraRotation.x > 180 && cameraRotation.x <= 360)
            {
                float negatedAngle = cameraRotation.x - 360;
                if (negatedAngle < _minCameraAngle)
                {
                    cameraRotation.x = _minCameraAngle;
                }
            }

            transform.rotation = Quaternion.Euler(0, cameraRotation.y, 0);
            _cameraHolder.localRotation = Quaternion.Euler(cameraRotation.x, 0, 0);
        }

        #endregion Camera Control

        #region Inputs

        private void HandleMouseInput()
        {
            _mouseInput.x = Input.GetAxisRaw(InputKeys.MouseX);
            _mouseInput.y = Input.GetAxisRaw(InputKeys.MouseY);
        }

        #endregion Inputs
    }
}