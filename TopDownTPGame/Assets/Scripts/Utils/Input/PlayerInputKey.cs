using UnityEngine;

namespace Utils.Input
{
    public struct PlayerInputKey
    {
        public bool keyPressedThisFrame;
        public bool keyReleasedThisFrame;
        public bool keyPressed;
        public bool isDataRead;

        public void UpdateInputData(KeyCode key)
        {
            bool keyPressedThisFrame = UnityEngine.Input.GetKeyDown(key);
            bool keyReleasedThisFrame = UnityEngine.Input.GetKeyUp(key);

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

            keyPressed = UnityEngine.Input.GetKey(key);
        }

        public void ResetPerFrameInput()
        {
            keyPressedThisFrame = false;
            keyReleasedThisFrame = false;
            isDataRead = true;
        }
    }
}