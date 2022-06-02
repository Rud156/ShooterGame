using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(BasePlayerController))]
    public class PlayerAnimationController : MonoBehaviour
    {
        private readonly int HORIZONTAL_ANIM_PARAM = Animator.StringToHash("Horizontal");
        private readonly int VERTICAL_ANIM_PARAM = Animator.StringToHash("Vertical");
        private readonly int IDLE_ANIM_PARAM = Animator.StringToHash("Idle");
        private readonly int WALK_ANIM_PARAM = Animator.StringToHash("Walk");
        private readonly int RUN_ANIM_PARAM = Animator.StringToHash("Run");
        private readonly int CROUCH_ANIM_PARAM = Animator.StringToHash("Crouch");
        private readonly int JUMP_ANIM_PARAM = Animator.StringToHash("Jump");
        private readonly int SLIDE_ANIM_PARAM = Animator.StringToHash("Slide");
        private readonly int FALLING_ANIM_PARAM = Animator.StringToHash("Falling");
        private readonly int LANDING_ANIM_PARAM = Animator.StringToHash("Land");

        [Header("Components")]
        [SerializeField] private Animator m_playerAnimator;

        private BasePlayerController m_playerController;

        #region Unity Functions

        private void Start()
        {
            m_playerController = GetComponent<BasePlayerController>();
            m_playerController.OnPlayerStatePushed += HandlePlayerStatePushed;
            m_playerController.OnPlayerStatePopped += HandlePlayerStatePopped;
            m_playerController.OnPlayerJumped += HandleJumpPressed;
            m_playerController.OnPlayerGroundedChanged += HandleGroundedStatusChanged;
        }

        private void OnDestroy()
        {
            m_playerController.OnPlayerStatePushed -= HandlePlayerStatePushed;
            m_playerController.OnPlayerStatePopped -= HandlePlayerStatePopped;
            m_playerController.OnPlayerJumped -= HandleJumpPressed;
            m_playerController.OnPlayerGroundedChanged -= HandleGroundedStatusChanged;
        }

        private void FixedUpdate()
        {
            Vector2 playerHorizontalInput = m_playerController.GetPlayerInputVector();
            m_playerAnimator.SetFloat(HORIZONTAL_ANIM_PARAM, playerHorizontalInput.x);
            m_playerAnimator.SetFloat(VERTICAL_ANIM_PARAM, playerHorizontalInput.y);
        }

        #endregion Unity Functions

        #region Animations

        private void HandlePlayerStatePushed(BasePlayerController.PlayerState pushedState)
        {
            SetAnimatorBoolStateActive(pushedState);
            Debug.Log($"Player State Pushed: {pushedState}");
        }

        private void HandlePlayerStatePopped(BasePlayerController.PlayerState poppedState, BasePlayerController.PlayerState nextTopState)
        {
            SetAnimatorBoolStateActive(nextTopState);
            Debug.Log($"Player State Popped: {poppedState}");
        }

        private void SetAnimatorBoolStateActive(BasePlayerController.PlayerState playerState)
        {
            m_playerAnimator.SetBool(IDLE_ANIM_PARAM, false);
            m_playerAnimator.SetBool(WALK_ANIM_PARAM, false);
            m_playerAnimator.SetBool(RUN_ANIM_PARAM, false);
            m_playerAnimator.SetBool(CROUCH_ANIM_PARAM, false);
            m_playerAnimator.SetBool(SLIDE_ANIM_PARAM, false);
            m_playerAnimator.SetBool(FALLING_ANIM_PARAM, false);

            switch (playerState)
            {
                case BasePlayerController.PlayerState.Idle:
                    m_playerAnimator.SetBool(IDLE_ANIM_PARAM, true);
                    break;

                case BasePlayerController.PlayerState.Walk:
                    m_playerAnimator.SetBool(WALK_ANIM_PARAM, true);
                    break;

                case BasePlayerController.PlayerState.Run:
                    m_playerAnimator.SetBool(RUN_ANIM_PARAM, true);
                    break;

                case BasePlayerController.PlayerState.Crouch:
                    m_playerAnimator.SetBool(CROUCH_ANIM_PARAM, true);
                    break;

                case BasePlayerController.PlayerState.Slide:
                    m_playerAnimator.SetBool(SLIDE_ANIM_PARAM, true);
                    break;

                case BasePlayerController.PlayerState.Falling:
                    m_playerAnimator.SetBool(FALLING_ANIM_PARAM, true);
                    break;
            }
        }

        private void HandleJumpPressed()
        {
            m_playerAnimator.SetTrigger(JUMP_ANIM_PARAM);
            Debug.Log("Player Jumped");
        }

        private void HandleGroundedStatusChanged(bool previousGrounded, bool currentGrounded)
        {
            // This means that the player just landed...
            if (!previousGrounded && currentGrounded)
            {
                m_playerAnimator.SetTrigger(LANDING_ANIM_PARAM);
                Debug.Log($"Player Landed. Previous State: {previousGrounded}, Current State: {currentGrounded}");
            }
        }

        #endregion Animations
    }
}