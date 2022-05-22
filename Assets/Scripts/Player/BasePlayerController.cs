using UnityEngine;
using Utils;

namespace Player
{
    public class BasePlayerController : MonoBehaviour
    {
        [Header("Basic Movement")]
        [SerializeField] private float m_horizontalRunSpeed;
        [SerializeField] private float m_horizontalWalkSpeed;
        [SerializeField] private float m_rotationSpeed;
        [SerializeField] private float m_maxCameraXAngle;
        [SerializeField] private float m_minCameraXAngle;

        [Header("Components")]
        [SerializeField] private Transform m_cameraTransform;

        private CharacterController m_characterController;
        private Vector3 m_characterVelocity = Vector3.zero;
        private Vector2 m_cameraRotation = Vector2.zero;

        private Vector2 m_horizontalInput = Vector2.zero;
        private Vector2 m_mouseInput = Vector2.zero;
        private bool m_isRunKeyPressed = false;

        #region Unity Functions

        private void Start()
        {
            m_characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            UpdateBasicKeyboardInput();
            UpdateMouseInput();
        }

        private void FixedUpdate()
        {
            UpdateHorizontalMovement();
            UpdateMouseMovement();
        }

        #endregion Unity Functions

        #region Player Horizontal Movement

        private void UpdateHorizontalMovement()
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            Vector3 groundedMovement = forward * m_horizontalInput.y + right * m_horizontalInput.x;
            groundedMovement.y = 0;
            groundedMovement = groundedMovement.normalized * (m_isRunKeyPressed ? m_horizontalRunSpeed : m_horizontalWalkSpeed);

            m_characterVelocity.x = groundedMovement.x;
            m_characterVelocity.z = groundedMovement.z;
            m_characterController.Move(m_characterVelocity * Time.fixedDeltaTime);
        }

        #endregion Player Horizontal Movement

        #region Player Mouse Movement

        private void UpdateMouseMovement()
        {
            m_cameraRotation.y += m_mouseInput.x * m_rotationSpeed * Time.fixedDeltaTime;
            m_cameraRotation.x += m_mouseInput.y * m_rotationSpeed * Time.fixedDeltaTime;
            m_cameraRotation.x = ExtensionFunctions.To360Angle(m_cameraRotation.x);

            // Clamp X Rotation
            if (m_cameraRotation.x >= 0 && m_cameraRotation.x <= 180)
            {
                if (m_cameraRotation.x > m_maxCameraXAngle)
                {
                    m_cameraRotation.x = m_maxCameraXAngle;
                }
            }
            else if (m_cameraRotation.x > 180 && m_cameraRotation.x <= 360)
            {
                float negatedAngle = m_cameraRotation.x - 360;
                if (negatedAngle < m_minCameraXAngle)
                {
                    m_cameraRotation.x = m_minCameraXAngle;
                }
            }

            transform.rotation = Quaternion.Euler(0, m_cameraRotation.y, 0);
            m_cameraTransform.localRotation = Quaternion.Euler(m_cameraRotation.x, 0, 0);
        }

        #endregion Player Mouse Movement

        #region Input

        private void UpdateBasicKeyboardInput()
        {
            float moveX = Input.GetAxisRaw(InputKeys.Horizontal);
            float moveZ = Input.GetAxisRaw(InputKeys.Vertical);
            m_horizontalInput.x = moveX;
            m_horizontalInput.y = moveZ;

            m_isRunKeyPressed = Input.GetKey(InputKeys.Run);
        }

        private void UpdateMouseInput()
        {
            float mouseX = Input.GetAxisRaw(InputKeys.MouseX);
            float mouseY = -Input.GetAxisRaw(InputKeys.MouseY);
            m_mouseInput.x = mouseX;
            m_mouseInput.y = mouseY;
        }

        #endregion Input
    }
}