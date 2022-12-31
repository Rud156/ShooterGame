using UnityEngine;

namespace Utils.Input
{
    public struct PlayerInputKey
    {
        public bool KeyPressedThisFrame;
        public bool KeyReleasedThisFrame;
        public bool KeyPressed;

        public void UpdateInputData(KeyCode key, KeyCode secondKey = KeyCode.None)
        {
            var keyPressedThisFrame = UnityEngine.Input.GetKeyDown(key) || UnityEngine.Input.GetKeyDown(secondKey);
            var keyReleasedThisFrame = UnityEngine.Input.GetKeyUp(key) || UnityEngine.Input.GetKeyUp(secondKey);

            if (keyPressedThisFrame && !KeyPressedThisFrame)
            {
                KeyPressedThisFrame = true;
            }

            if (keyReleasedThisFrame && !KeyReleasedThisFrame)
            {
                KeyReleasedThisFrame = true;
            }

            KeyPressed = UnityEngine.Input.GetKey(key) || UnityEngine.Input.GetKey(secondKey);
        }

        public void ResetPerFrameInput()
        {
            KeyPressedThisFrame = false;
            KeyReleasedThisFrame = false;
        }
    }
}