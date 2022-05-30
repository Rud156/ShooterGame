using System.Collections.Generic;
using UnityEngine;
using Utils.Input;
using Utils.Misc;

namespace Player
{
    public class BasePlayerController : MonoBehaviour
    {
        private const float GROUND_ANGLE_NORMAL = 90;

        [Header("Basic Movement")]
        [SerializeField] private float m_horizontalRunSpeed;

        [SerializeField] private float m_horizontalWalkSpeed;
        [SerializeField] private float m_defaultCapsuleHeight;
        [SerializeField] private float m_rotationSpeed;
        [SerializeField] private float m_maxCameraXAngle;
        [SerializeField] private float m_minCameraXAngle;

        [Header("Jump/Grounded")]
        [SerializeField] private float m_jumpVelocity;

        [SerializeField] private float m_gravityMultiplier;
        [SerializeField] private LayerMask m_groundedCheckMask;
        [SerializeField] private float m_groundedCheckDistance;

        [Header("Air Control")]
        [SerializeField] private float m_airControlMultiplier;

        [Header("Crouch")]
        [SerializeField] private float m_crouchWalkSpeed;

        [SerializeField] private float m_crouchCapsuleHeight;

        [Header("Slide")]
        [SerializeField] private float m_slideSpeed;

        [SerializeField] private float m_slideDuration;
        [SerializeField] private Transform m_slideWallCheckPoint;
        [SerializeField] private float m_slideWallCheckDistance;
        [SerializeField] private Transform m_slideSlopeCheckPoint;
        [SerializeField] private float m_slideNormalCheckDistance;
        [SerializeField] private float m_slideMinSlopeExtensionAngle;
        [SerializeField] private float m_slideMaxSlopeExtensionAngle;
        [SerializeField] private float m_slideMaxSlopeSpeed;
        [SerializeField] private float m_slideUpwardTimeReductionMultiplier;

        [Header("Components")]
        [SerializeField] private float m_capsuleLerpSpeed;

        [SerializeField] private Transform m_cameraTransform;
        [SerializeField] private Transform m_groundedCheckPoint;
        [SerializeField] private List<Transform> m_rayCastTransforms;

        private CharacterController m_characterController;
        private Vector3 m_characterVelocity = Vector3.zero;

        private Vector2 m_horizontalInput = Vector2.zero;
        private Vector2 m_mouseInput = Vector2.zero;
        private PlayerInputKey m_runKey;
        private PlayerInputKey m_jumpKey;
        private PlayerInputKey m_crouchKey;
        private float m_currentStateMoveVelocity;

        private float m_capsuleStartHeight = 0;
        private float m_capsuleTargetHeight = 0;
        private float m_capsuleLerpAmount = 0;

        private float m_currentSlideDuration = 0;

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

            m_runKey = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            m_crouchKey = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };
            m_jumpKey = new PlayerInputKey() { keyPressed = false, keyReleasedThisFrame = false, keyPressedThisFrame = false, isDataRead = true };

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
            // If we want additional Movement Tech then apply them here by going through a Movement Script List
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

                case PlayerState.Falling:
                    UpdateFallingState();
                    break;
            }

            UpdateHorizontalMovement();
            UpdateCapsuleSizeAndPosition();
        }

        #region Player Horizontal Movement

        private void UpdateIdleState()
        {
            m_currentStateMoveVelocity = 0;

            if (m_crouchKey.keyPressedThisFrame)
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

            if (m_runKey.keyPressedThisFrame)
            {
                PushTopPlayerState(PlayerState.Run);
            }
            else if (m_crouchKey.keyPressedThisFrame)
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

            if (HasNoDirectionalInput() || m_horizontalInput.y <= 0 || m_runKey.keyPressedThisFrame)
            {
                PopTopPlayerState();
            }
            else if (m_crouchKey.keyPressedThisFrame)
            {
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

            if (m_runKey.keyPressedThisFrame)
            {
                PopTopPlayerState();
                PushTopPlayerState(PlayerState.Run);
            }
            else if (m_crouchKey.keyPressedThisFrame)
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

            // Stop slide when the Raycast hits wall or something...
            if (Physics.Raycast(m_slideWallCheckPoint.position, transform.forward, m_slideWallCheckDistance, m_groundedCheckMask))
            {
                m_currentSlideDuration = 0;
            }

            // Change Sliding Speed and Duration Based on Slope
            if (Physics.Raycast(m_slideSlopeCheckPoint.position, Vector3.down, out RaycastHit hit, m_slideNormalCheckDistance, m_groundedCheckMask))
            {
                Debug.DrawRay(m_slideSlopeCheckPoint.position, Vector3.down * m_slideNormalCheckDistance, Color.green);
                float normalAngle = Vector3.Angle(transform.forward, hit.normal);
                normalAngle = GROUND_ANGLE_NORMAL - normalAngle;

                if (normalAngle > m_slideMinSlopeExtensionAngle)
                {
                    float mappedSlidingSpeed = ExtensionFunctions.Map(normalAngle, m_slideMinSlopeExtensionAngle, m_slideMaxSlopeExtensionAngle,
                                                m_slideSpeed, m_slideMaxSlopeSpeed);
                    m_currentStateMoveVelocity = mappedSlidingSpeed;
                    m_currentSlideDuration = m_slideDuration;
                }
                else if (normalAngle < 0) // This means they are sliding up a hill
                {
                    m_currentSlideDuration -= Time.fixedDeltaTime * m_slideUpwardTimeReductionMultiplier;
                }
            }

            if (m_currentSlideDuration <= 0)
            {
                RemovePlayerState(PlayerState.Run);
                PopTopPlayerState();
                PushTopPlayerState(PlayerState.Crouch);
            }
            else if (m_runKey.keyPressedThisFrame)
            {
                PopTopPlayerState();
            }
        }

        private void UpdateHorizontalMovement()
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            if (m_playerStateStack[^1] != PlayerState.Falling)
            {
                Vector3 groundedMovement = forward * m_horizontalInput.y + right * m_horizontalInput.x;
                groundedMovement.y = 0;
                groundedMovement = groundedMovement.normalized * m_currentStateMoveVelocity;

                m_characterVelocity.x = groundedMovement.x;
                m_characterVelocity.z = groundedMovement.z;
            }
            else
            {
                Vector3 airMovement = forward * m_horizontalInput.y + right * m_horizontalInput.x;
                airMovement.y = 0;
                airMovement = m_airControlMultiplier * m_currentStateMoveVelocity * airMovement.normalized;

                airMovement.x += m_characterVelocity.x;
                airMovement.z += m_characterVelocity.z;
                airMovement = airMovement.normalized * m_currentStateMoveVelocity;

                m_characterVelocity.x = airMovement.x;
                m_characterVelocity.z = airMovement.z;
            }
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
            float computedHeight = (lerpedHeight - lastHeight) / 2;

            m_characterController.transform.position = new Vector3(position.x, position.y + computedHeight, position.z);
            foreach (Transform rayCastTransform in m_rayCastTransforms)
            {
                Vector3 localPosition = rayCastTransform.localPosition;
                rayCastTransform.localPosition = new Vector3(localPosition.x, localPosition.y - computedHeight, localPosition.z);
            }
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

            if (!isGrounded && m_playerStateStack[^1] != PlayerState.Falling)
            {
                PushTopPlayerState(PlayerState.Falling);
            }

            m_isGrounded = isGrounded;
        }

        private void ProcessJumpInput()
        {
            bool isValidJumpPressed = m_jumpKey.keyPressedThisFrame;
            if (!isValidJumpPressed || !m_isGrounded)
            {
                return;
            }

            if (m_playerStateStack[^1] == PlayerState.Crouch)
            {
                PopTopPlayerState();
            }
            else if (m_playerStateStack[^1] == PlayerState.Slide)
            {
                PopTopPlayerState();
            }

            m_characterVelocity.y += m_jumpVelocity;
        }

        private void UpdateFallingState()
        {
            if (m_isGrounded)
            {
                PopTopPlayerState();
            }
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

            m_runKey.UpdateInputData(InputKeys.Run);
            m_jumpKey.UpdateInputData(InputKeys.Jump);
            m_crouchKey.UpdateInputData(InputKeys.Crouch);
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

            m_runKey.ResetPerFrameInput();
            m_jumpKey.ResetPerFrameInput();
            m_crouchKey.ResetPerFrameInput();
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

                case PlayerState.Falling:
                    break;
            }
        }

        private void PopTopPlayerState()
        {
            PlayerState topState = m_playerStateStack[^1];
            ResetPlayerStatePoppedData(topState);
            m_playerStateStack.RemoveAt(m_playerStateStack.Count - 1);
            SetupCapsuleSizeForState();
        }

        private PlayerState GetPlayerStateAtIndex(int index) => m_playerStateStack[index];

        private void RemovePlayerState(PlayerState playerState)
        {
            bool removed = false;
            for (int i = m_playerStateStack.Count - 1; i >= 0; i--)
            {
                if (m_playerStateStack[i] == playerState)
                {
                    removed = true;
                    m_playerStateStack.RemoveAt(i);
                }
            }

            if (removed)
            {
                ResetPlayerStatePoppedData(playerState);
                SetupCapsuleSizeForState();
            }
        }

        private void ResetPlayerStatePoppedData(PlayerState playerState)
        {
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
                    {
                        // Reset Camera after slide
                        float cameraXRotation = m_cameraTransform.localRotation.eulerAngles.x;
                        transform.rotation = Quaternion.Euler(0, m_cameraTransform.rotation.eulerAngles.y, 0);
                        m_cameraTransform.localRotation = Quaternion.Euler(cameraXRotation, 0, 0);
                    }
                    break;

                case PlayerState.Falling:
                    break;
            }
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

                case PlayerState.Falling:
                    m_capsuleTargetHeight = m_defaultCapsuleHeight;
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
            Slide,
            Falling
        };

        #endregion Player State
    }
}