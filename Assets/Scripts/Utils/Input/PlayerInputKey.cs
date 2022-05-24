using System;

namespace Utils.Input
{
    public struct PlayerInputKey
    {
        public bool keyPressed;
        public bool isNewState;

        public PlayerInputKey(bool keyPressed, bool isNewState)
        {
            this.keyPressed = keyPressed;
            this.isNewState = isNewState;
        }
    }
}