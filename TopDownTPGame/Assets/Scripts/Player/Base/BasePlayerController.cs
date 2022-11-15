using UnityEngine;

namespace Player.Base
{
    [RequireComponent(typeof(CharacterController))]
    public class BasePlayerController : MonoBehaviour
    {
        [Header("Basic Move")]
        [SerializeField] private float m_runSpeed;
        [SerializeField] private float m_walkSpeed;

        // Input
        private Vector2 _coreMoveInput;

        // Movement/Controller
        private CharacterController _characterController;
        private Vector3 _characterVelocity;

        #region Unity Functions

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            HandleKeyboardInput();
        }

        private void FixedUpdate()
        {
            UpdateCoreMovement();
            ApplyFinalMovement();
        }

        #endregion

        #region Core Movement

        private void UpdateCoreMovement()
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            Vector3 groundedMovement = forward * _coreMoveInput.y + right * _coreMoveInput.x;
            groundedMovement.y = 0;
            groundedMovement = groundedMovement.normalized * m_walkSpeed;

            _characterVelocity.x = groundedMovement.x;
            _characterVelocity.z = groundedMovement.z;
        }

        private void ApplyFinalMovement() => _characterController.Move(_characterVelocity * Time.fixedDeltaTime);

        #endregion

        #region Inputs

        private void HandleKeyboardInput()
        {
            _coreMoveInput = new Vector2()
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = Input.GetAxisRaw("Vertical")
            };
        }

        #endregion
    }
}
