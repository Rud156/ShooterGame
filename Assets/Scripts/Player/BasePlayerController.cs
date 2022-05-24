using System.Collections.Generic;
using UnityEngine;
using Utils;
using Utils.Input;
using Utils.Misc;

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
        [SerializeField] private float m_crouchCapsuleHeight;
        [SerializeField] private float m_slideSpeed;
        [SerializeField] private float m_slideDuration;

        [Header("Components")]
        [SerializeField] private float m_capsuleLerpSpeed;
        [SerializeField] private Transform m_cameraTransform;
        [SerializeField] private Transform m_groundedCheckPoint;

        private CharacterController m_characterController;
        private Vector3 m_characterVelocity = Vector3.zero;

        private Vector2 m_horizontalInput = Vector2.zero;
        private Vector2 m_mouseInput = Vector2.zero;
        private PlayerInputKey m_isRunKeyPressed;
        private PlayerInputKey m_isJumpKeyPressed;
        private PlayerInputKey m_isCrouchPressed;
        public float m_currentStateMoveVelocity;

        private float m_capsuleStartHeight = 0;
        private float m_capsuleTargetHeight = 0;
        private float m_capsuleLerpAmount = 0;

        public float m_currentSlideDuration = 0;

        public List<PlayerState> m_playerStateStack;
        private bool m_isGrounded = false;

        #region Unity Functions

        private void Start()
        {
            m_characterController = GetComponent<CharacterController>();
            m_playerStateStack = new List<PlayerState>();

            m_capsuleStartHeight = 2;
            m_capsuleTargetHeight = 2;
            m_capsuleLerpAmount = 1;

            m_isRunKeyPressed = new PlayerInputKey() { isNewState = false, keyPressed = false };
            m_isCrouchPressed = new PlayerInputKey() { isNewState = false, keyPressed = false };
            m_isJumpKeyPressed = new PlayerInputKey() { isNewState = false, keyPressed = false };

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

            MarkFrameInputsAsRead();
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
            m_currentStateMoveVelocity = 0;

            if (m_isCrouchPressed.isNewState && m_isCrouchPressed.keyPressed)
            {
                PushTopPlayerState(PlayerState.Crouch);
            }
            else if (!HasNoDirectionalInput())
            {
                PushTopPlayerState(PlayerState.Walk);
            }
        }

        private void UpdateWalkState()
        {
            m_currentStateMoveVelocity = m_horizontalWalkSpeed;
            if (!m_isGrounded)
            {
                m_currentStateMoveVelocity *= m_airControlMultiplier;
            }

            if (m_isRunKeyPressed.isNewState && m_isRunKeyPressed.keyPressed)
            {
                PushTopPlayerState(PlayerState.Run);
            }
            else if (m_isCrouchPressed.isNewState && m_isCrouchPressed.keyPressed)
            {
                PushTopPlayerState(PlayerState.Crouch);
            }
            else if (HasNoDirectionalInput())
            {
                PopTopPlayerState();
            }
        }

        private void UpdateRunState()
        {
            m_currentStateMoveVelocity = m_horizontalRunSpeed;
            if (!m_isGrounded)
            {
                m_currentStateMoveVelocity *= m_airControlMultiplier;
            }

            if (HasNoDirectionalInput() || (m_isRunKeyPressed.isNewState && !m_isRunKeyPressed.keyPressed))
            {
                PopTopPlayerState();
            }
            else if (m_isCrouchPressed.isNewState && m_isCrouchPressed.keyPressed)
            {
                m_isRunKeyPressed = new PlayerInputKey() { isNewState = true, keyPressed = false };
                PopTopPlayerState();
                PushTopPlayerState(PlayerState.Slide);
            }
        }

        private void UpdateCrouchState()
        {
            m_currentStateMoveVelocity = m_crouchWalkSpeed;
            if (HasNoDirectionalInput())
            {
                m_currentStateMoveVelocity = 0;
            }

            if (m_isRunKeyPressed.isNewState && m_isRunKeyPressed.keyPressed)
            {
                PopTopPlayerState();
                PushTopPlayerState(PlayerState.Run);
            }
            else if (m_isCrouchPressed.isNewState && !m_isCrouchPressed.keyPressed)
            {
                PopTopPlayerState();
            }
        }

        private void UpdateSlideState()
        {
            m_horizontalInput.x = 0;
            m_horizontalInput.y = 1;

            float slideDurationRatio = 1 - m_currentSlideDuration / m_slideDuration;
            float mappedSlideSpeed = Mathf.Lerp(m_slideSpeed, 0, slideDurationRatio);
            m_currentStateMoveVelocity = mappedSlideSpeed;
            m_currentSlideDuration -= Time.fixedDeltaTime;

            if (m_currentSlideDuration <= 0)
            {
                PopTopPlayerState();
                PushTopPlayerState(PlayerState.Crouch);
            }
            else if (m_isRunKeyPressed.isNewState && m_isRunKeyPressed.isNewState)
            {
                PopTopPlayerState();
                PushTopPlayerState(PlayerState.Slide);
            }
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
            bool isValidJumpPressed = m_isJumpKeyPressed.isNewState && m_isJumpKeyPressed.keyPressed;
            if (!isValidJumpPressed || !m_isGrounded)
            {
                return;
            }

            if (m_playerStateStack[^1] == PlayerState.Crouch)
            {
                PopTopPlayerState();
                return;
            }
            else if (m_playerStateStack[^1] == PlayerState.Slide)
            {
                PopTopPlayerState();
            }

            m_characterVelocity.y += m_jumpVelocity;
            m_isJumpKeyPressed = new PlayerInputKey() { isNewState = true, keyPressed = false };
        }

        private void ProcessGlobalGravity()
        {
            if (!m_isGrounded)
            {
                m_characterVelocity.y += Physics.gravity.y * m_gravityMultiplier;
            }
        }

        private void ApplyFinalPlayerMovement() => m_characterController.Move(m_characterVelocity * Time.fixedDeltaTime);

        #endregion Player Vertical Movement

        #region Player Mouse Movement

        private void UpdateMouseMovement()
        {
            Vector3 cameraRotation = m_cameraTransform.rotation.eulerAngles;
            cameraRotation.y += m_mouseInput.x * m_rotationSpeed * Time.fixedDeltaTime;
            cameraRotation.x += m_mouseInput.y * m_rotationSpeed * Time.fixedDeltaTime;
            cameraRotation.x = ExtensionFunctions.To360Angle(cameraRotation.x);

            // Clamp X Rotation
            if (cameraRotation.x >= 0 && cameraRotation.x <= 180)
            {
                if (cameraRotation.x > m_maxCameraXAngle)
                {
                    cameraRotation.x = m_maxCameraXAngle;
                }
            }
            else if (cameraRotation.x > 180 && cameraRotation.x <= 360)
            {
                float negatedAngle = cameraRotation.x - 360;
                if (negatedAngle < m_minCameraXAngle)
                {
                    cameraRotation.x = m_minCameraXAngle;
                }
            }

            // Only move the camera when sliding...
            if (m_playerStateStack[^1] == PlayerState.Slide)
            {
                m_cameraTransform.rotation = Quaternion.Euler(0, cameraRotation.y, 0);
                float localYRotation = m_cameraTransform.localEulerAngles.y;
                m_cameraTransform.localRotation = Quaternion.Euler(cameraRotation.x, localYRotation, 0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, cameraRotation.y, 0);
                m_cameraTransform.localRotation = Quaternion.Euler(cameraRotation.x, 0, 0);
            }
        }

        #endregion Player Mouse Movement

        #endregion Player Movement

        #region Input

        private void UpdateBasicKeyboardInput()
        {
            float moveX = Input.GetAxisRaw(InputKeys.Horizontal);
            float moveZ = Input.GetAxisRaw(InputKeys.Vertical);
            m_horizontalInput.x = moveX;
            m_horizontalInput.y = moveZ;

            if (moveZ <= 0)
            {
                m_isRunKeyPressed = new PlayerInputKey() { isNewState = true, keyPressed = false };
            }
            else if (Input.GetKeyDown(InputKeys.Run))
            {
                m_isRunKeyPressed = new PlayerInputKey() { isNewState = true, keyPressed = !m_isRunKeyPressed.keyPressed };
            }

            if (Input.GetKeyDown(InputKeys.Jump))
            {
                m_isJumpKeyPressed = new PlayerInputKey() { isNewState = true, keyPressed = true };
            }

            if (Input.GetKeyDown(InputKeys.Crouch))
            {
                m_isCrouchPressed = new PlayerInputKey() { isNewState = true, keyPressed = !m_isCrouchPressed.keyPressed };
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

        private void MarkFrameInputsAsRead()
        {
            m_mouseInput.x = 0;
            m_mouseInput.y = 0;

            m_horizontalInput.x = 0;
            m_horizontalInput.y = 0;
            m_isRunKeyPressed.isNewState = false;
            m_isJumpKeyPressed.isNewState = false;
            m_isCrouchPressed.isNewState = false;
        }

        #endregion Input

        #region Player State

        private void PushTopPlayerState(PlayerState playerState)
        {
            m_playerStateStack.Add(playerState);
            SetupCapsuleSizeForState();

            switch (playerState)
            {
                case PlayerState.Idle:
                    break;

                case PlayerState.Walk:
                    break;

                case PlayerState.Run:
                    break;

                case PlayerState.Crouch:
                    break;

                case PlayerState.Slide:
                    m_currentSlideDuration = m_slideDuration;
                    break;
            }

            Debug.Log($"Pushed State: {playerState}");
        }

        private void PopTopPlayerState()
        {
            PlayerState topState = m_playerStateStack[^1];
            switch (topState)
            {
                case PlayerState.Idle:
                    break;

                case PlayerState.Walk:
                    break;

                case PlayerState.Run:
                    m_isRunKeyPressed = new PlayerInputKey() { isNewState = true, keyPressed = false };
                    break;

                case PlayerState.Crouch:
                    m_isCrouchPressed = new PlayerInputKey() { isNewState = true, keyPressed = false };
                    break;

                case PlayerState.Slide:
                    {
                        // Reset Camera after slide
                        float cameraXRotation = m_cameraTransform.localRotation.eulerAngles.x;
                        transform.rotation = Quaternion.Euler(0, m_cameraTransform.rotation.y, 0);
                        Debug.Log($"Global Rotation: {m_cameraTransform.rotation.y}, Local Rotation: {m_cameraTransform.localRotation.y}");
                        m_cameraTransform.localRotation = Quaternion.Euler(cameraXRotation, 0, 0);
                    }
                    break;
            }

            m_playerStateStack.RemoveAt(m_playerStateStack.Count - 1);
            SetupCapsuleSizeForState();

            Debug.Log($"Popped State: {topState}");
        }

        private void SetupCapsuleSizeForState()
        {
            m_capsuleStartHeight = m_characterController.height;
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

        #endregion Player State
    }
}