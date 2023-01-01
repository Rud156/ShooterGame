#region

using UnityEngine;

#endregion

namespace Utils.Input
{
    public class InputManager : MonoBehaviour
    {
        private InputMaster _inputMaster;

        #region Input Setup

        private void SetupInput() => _inputMaster = new InputMaster();

        #endregion Input Setup

        #region External Functions

        public InputMaster InputMaster => _inputMaster;

        public InputMaster.PlayerActions PlayerInput => _inputMaster.Player;

        public void EnablePlayerControls() => _inputMaster.Player.Enable();

        public void DisablePlayerControls() => _inputMaster.Player.Disable();

        #endregion External Functions

        #region Singleton

        public static InputManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            SetupInput();
        }

        #endregion Singleton
    }
}