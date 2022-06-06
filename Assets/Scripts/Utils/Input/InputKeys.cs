using UnityEngine;

namespace Utils.Input
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
        public const KeyCode Crouch = KeyCode.LeftControl;

        // Weapon Pickup Key
        public const KeyCode WeaponPickup = KeyCode.E;
        public const KeyCode WeaponDrop = KeyCode.G;
    }
}