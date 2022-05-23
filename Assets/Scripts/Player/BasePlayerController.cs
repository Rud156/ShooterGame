using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Player
{
    public class BasePlayerController : MonoBehaviour
    {
        [Header("Basic Movement")]
        [SerializeField] private float m_horizontalRunSpeed;
        [SerializeField] private float m_horizontalWalkSpeed;
        [SerializeField] private float m_defaultCapsuleHeight;
        [SerializeField] private float m_rotationSpeed;
        [SerializeField] private float m_maxCameraXAngle;
        [SerializeField] private float m_minCameraXAngle;

        [Header("Jump/Grounded")]
        [SerializeField] private float m_airControlMultiplier;
        [SerializeField] private float m_jumpVelocity;
        [SerializeField] private float m_gravityMultiplier;
        [SerializeField] private LayerMask m_groundedCheckMask;
        [SerializeField] private float m_groundedCheckDistance;

        [Header("Crouch/Slide")]
        [SerializeField] private float m_crouchWalkSpeed;
        [SerializeField] private float m_slideSpeed;
        [SerializeField] private float m_crouchCapsuleHeight;

        [Header("Components")]
        [SerializeField] private float m_capsuleLerpSpeed;
        [SerializeField] private Transform m_cameraTransform;
        [SerializeField] private Transform m_groundedCheckPoint;

        private CharacterController m_characterController;
        private Vector3 m_characterVelocity = Vector3.zero;
        private Vector2 m_cameraRotation = Vector2.zero;

        private Vector2 m_horizontalInput = Vector2.zero;
        private Vector2 m_mouseInput = Vector2.zero;
        private bool m_isRunKeyPressed = false;
        private bool m_isJumpKeyPressed = false;
        private bool m_isCrouchPressed = false;
        private float m_currentStateMoveVelocity;

        private float m_capsuleStartHeight = 0;
        private float m_capsuleTargetHeight = 0;
        private float m_capsuleLerpAmount = 0;

        private List<PlayerState> m_playerStateStack;
        private bool m_isGrounded = false;

        #region Unity Functions

        private void Start()
        {
            m_characterController = GetComponent<CharacterController>();
            m_playerStateStack = new List<PlayerState>();

            m_capsuleStartHeight = 2;
            m_capsuleTargetHeight = 2;
            m_capsuleLerpAmount = 1;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            PushTopPlayerState(PlayerState.Idle);
        }

        private void Update()
        {
            UpdateBasicKeyboardInput();
            UpdateMouseInput();
        }

        private void FixedUpdate()
        {
            UpdateMouseMovement();
            UpdateGroundedState();

            UpdatePlayerMovement();
            ProcessJumpInput();
            ProcessGlobalGravity();
            ApplyFinalPlayerMovement();
        }

        #endregion Unity Functions

        #region Player Movement

        private void UpdatePlayerMovement()
        {
            switch (m_playerStateStack[^1])
            {
                case PlayerState.Idle:
                    UpdateIdleState();
                    break;

                case PlayerState.Walk:
                    UpdateWalkState();
                    break;

                case PlayerState.Run:
                    UpdateRunState();
                    break;

                case PlayerState.Crouch:
                    UpdateCrouchState();
                    break;

                case PlayerState.Slide:
                    UpdateSlideState();
                    break;
            }

            UpdateHorizontalMovement();
            UpdateCapsuleSizeAndPosition();
        }

        #region Player Horizontal Movement

        private void UpdateIdleState()
        {
            if (m_isCrouchPressed)
            {
                PushTopPlayerState(PlayerState.Crouch);
            }
            else if (!HasNoDirectionalInput())
            {
                PushTopPlayerState(PlayerState.Walk);
            }

            m_currentStateMoveVelocity = 0;
        }

        private void UpdateWalkState()
        {
            if (m_isRunKeyPressed)
            {
                PushTopPlayerState(PlayerState.Run);
            }
            else if (m_isCrouchPressed)
            {
                PushTopPlayerState(PlayerState.Crouch);
            }
            else if (HasNoDirectionalInput())
            {
                PopTopPlayerState();
            }

            m_currentStateMoveVelocity = m_horizontalWalkSpeed;
            if (!m_isGrounded)
            {
                m_currentStateMoveVelocity *= m_airControlMultiplier;
            }
        }

        private void UpdateRunState()
        {
            if (HasNoDirectionalInput() || !m_isRunKeyPressed)
            {
                PopTopPlayerState();
            }
            else if (m_isCrouchPressed)
            {
                // Put into slide state
                Debug.Log("Player Should Slide Here...");
            }

            m_currentStateMoveVelocity = m_horizontalRunSpeed;
            if (!m_isGrounded)
            {
                m_currentStateMoveVelocity *= m_airControlMultiplier;
            }
        }

        private void UpdateCrouchState()
        {
            if (m_isRunKeyPressed)
            {
                PopTopPlayerState();
                PushTopPlayerState(PlayerState.Run);
            }
            else if (!m_isCrouchPressed)
            {
                PopTopPlayerState();
            }

            m_currentStateMoveVelocity = m_crouchWalkSpeed;
            if (HasNoDirectionalInput())
            {
                m_currentStateMoveVelocity = 0;
            }
        }

        private void UpdateSlideState()
        {

        }

        private void UpdateHorizontalMovement()
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            Vector3 groundedMovement = forward * m_horizontalInput.y + right * m_horizontalInput.x;
            groundedMovement.y = 0;
            groundedMovement = groundedMovement.normalized * m_currentStateMoveVelocity;

            m_characterVelocity.x = groundedMovement.x;
            m_characterVelocity.z = groundedMovement.z;
        }

        private void UpdateCapsuleSizeAndPosition()
        {
            if (m_capsuleLerpAmount >= 1)
            {
                return;
            }

            m_capsuleLerpAmount += m_capsuleLerpSpeed * Time.fixedDeltaTime;
            if (m_capsuleLerpAmount >= 1)
            {
                m_capsuleLerpAmount = 1;
            }

            float lastHeight = m_characterController.height;
            float lerpedHeight = Mathf.Lerp(m_capsuleStartHeight, m_capsuleTargetHeight, m_capsuleLerpAmount);

            m_characterController.height = lerpedHeight;
            m_characterController.enabled = false;

            Vector3 position = m_characterController.transform.position;
            Vector3 groundCheckLocPosition = m_groundedCheckPoint.localPosition;
            float computedHeight = (lerpedHeight - lastHeight) / 2;

            m_characterController.transform.position = new Vector3(position.x, position.y + computedHeight, position.z);
            m_groundedCheckPoint.localPosition = new Vector3(groundCheckLocPosition.x, groundCheckLocPosition.y - computedHeight, groundCheckLocPosition.z);

            m_characterController.enabled = true;
        }

        #endregion Player Horizontal Movement

        #region Player Vertical Movement

        private void UpdateGroundedState()
        {
            bool isGrounded = Physics.Raycast(m_groundedCheckPoint.position, Vector3.down, m_groundedCheckDistance, m_groundedCheckMask);
            if (isGrounded && !m_isGrounded)
            {
                m_characterVelocity.y = 0;
            }
            m_isGrounded = isGrounded;
        }

        private void ProcessJumpInput()
        {
            if (!m_isJumpKeyPressed || !m_isGrounded)
            {
                return;
            }

            if (m_playerStateStack[^1] == PlayerState.Crouch)
            {
                PopTopPlayerState();
                return;
            }

            m_characterVelocity.y += m_jumpVelocity;
            m_isJumpKeyPressed = false;
        }

        private void ProcessGlobalGravity()
        {
            if (!m_isGrounded)
            {
                m_characterVelocity.y += Physics.gravity.y * m_gravityMultiplier;
            }
        }

        private void ApplyFinalPlayerMovement() => m_characterController.Move(m_characterVelocity * Time.fixedDeltaTime);

        #endregion

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

        #endregion

        #region Input

        private void UpdateBasicKeyboardInput()
        {
            float moveX = Input.GetAxisRaw(InputKeys.Horizontal);
            float moveZ = Input.GetAxisRaw(InputKeys.Vertical);
            m_horizontalInput.x = moveX;
            m_horizontalInput.y = moveZ;

            if (moveZ <= 0)
            {
                m_isRunKeyPressed = false;
            }
            else if (Input.GetKeyDown(InputKeys.Run))
            {
                m_isRunKeyPressed = !m_isRunKeyPressed;
            }

            if (Input.GetKeyDown(InputKeys.Jump))
            {
                m_isJumpKeyPressed = true;
            }

            if (Input.GetKeyDown(InputKeys.Crouch))
            {
                m_isCrouchPressed = !m_isCrouchPressed;
            }
        }

        private bool HasNoDirectionalInput() => ExtensionFunctions.IsNearlyEqual(m_horizontalInput.x, 0) && ExtensionFunctions.IsNearlyEqual(m_horizontalInput.y, 0);

        private void UpdateMouseInput()
        {
            float mouseX = Input.GetAxisRaw(InputKeys.MouseX);
            float mouseY = -Input.GetAxisRaw(InputKeys.MouseY);
            m_mouseInput.x = mouseX;
            m_mouseInput.y = mouseY;
        }

        private void ClearInputs()
        {
            m_mouseInput.x = 0;
            m_mouseInput.y = 0;

            m_horizontalInput.x = 0;
            m_horizontalInput.y = 0;
            m_isRunKeyPressed = false;
            m_isJumpKeyPressed = false;
            m_isCrouchPressed = false;
        }

        #endregion Input

        #region Player State

        private void PushTopPlayerState(PlayerState playerState)
        {
            m_playerStateStack.Add(playerState);
            SetupCapsuleSizeForState();
        }

        private void PopTopPlayerState()
        {
            PlayerState topState = m_playerStateStack[^1];
            if (topState == PlayerState.Crouch)
            {
                m_isCrouchPressed = false;
            }

            m_playerStateStack.RemoveAt(m_playerStateStack.Count - 1);
            SetupCapsuleSizeForState();
        }

        private void SetupCapsuleSizeForState()
        {
            m_capsuleStartHeight = m_capsuleTargetHeight;
            switch (m_playerStateStack[^1])
            {
                case PlayerState.Idle:
                    m_capsuleTargetHeight = m_defaultCapsuleHeight;
                    break;

                case PlayerState.Walk:
                    m_capsuleTargetHeight = m_defaultCapsuleHeight;
                    break;

                case PlayerState.Run:
                    m_capsuleTargetHeight = m_defaultCapsuleHeight;
                    break;

                case PlayerState.Crouch:
                    m_capsuleTargetHeight = m_crouchCapsuleHeight;
                    break;

                case PlayerState.Slide:
                    m_capsuleTargetHeight = m_crouchCapsuleHeight;
                    break;
            }

            m_capsuleLerpAmount = 0;
        }

        public enum PlayerState
        {
            Idle,
            Walk,
            Run,
            Crouch,
            Slide
        };

        #endregion
    }
}