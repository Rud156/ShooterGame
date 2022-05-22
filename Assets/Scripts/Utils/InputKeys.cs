using UnityEngine;

namespace Utils
{
    public class InputKeys : MonoBehaviour
    {
        // Movement Axis
        public const string Horizontal = "Horizontal";
        public const string Vertical = "Vertical";
        public const string MouseX = "Mouse X";
        public const string MouseY = "Mouse Y";

        // Movement Keys
        public const KeyCode Jump = KeyCode.Space;
        public const KeyCode Run = KeyCode.LeftShift;
        public const KeyCode Crouch = KeyCode.RightControl;
    }
}