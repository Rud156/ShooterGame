using UnityEngine;

namespace Utils.Input
{
    public struct PlayerInputKey
    {
        public bool keyPressedThisFrame;
        public bool keyReleasedThisFrame;
        public bool keyPressed;
        public bool isDataRead;

        public void UpdateInputData(KeyCode key, KeyCode secondKey = KeyCode.None)
        {
            bool keyPressedThisFrame = UnityEngine.Input.GetKeyDown(key) || UnityEngine.Input.GetKeyDown(secondKey);
            bool keyReleasedThisFrame = UnityEngine.Input.GetKeyUp(key) || UnityEngine.Input.GetKeyUp(secondKey);

            if (keyPressedThisFrame && !this.keyPressedThisFrame)
            {
                this.keyPressedThisFrame = keyPressedThisFrame;
                isDataRead = false;
            }

            if (keyReleasedThisFrame && !this.keyReleasedThisFrame)
            {
                this.keyReleasedThisFrame = keyReleasedThisFrame;
                isDataRead = false;
            }

            keyPressed = UnityEngine.Input.GetKey(key) || UnityEngine.Input.GetKey(secondKey);
        }

        public void ResetPerFrameInput()
        {
            keyPressedThisFrame = false;
            keyReleasedThisFrame = false;
            isDataRead = true;
        }
    }
}