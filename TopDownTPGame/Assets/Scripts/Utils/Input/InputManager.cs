using UnityEngine;

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

        private static InputManager _instance;

        public static InputManager Instance => _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            SetupInput();
        }

        #endregion Singleton
    }
}