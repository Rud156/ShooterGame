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
        public const KeyCode CameraSwitchLeftKey = KeyCode.Q;
        public const KeyCode CameraSwitchRightKey = KeyCode.E;

        // Weapon Pickup Key
        public const KeyCode WeaponPickup = KeyCode.F;
        public const KeyCode WeaponDrop = KeyCode.G;

        // Weapon Attack
        public const KeyCode AttackPrimary = KeyCode.Mouse0;
        public const KeyCode AttackSecondary = KeyCode.Mouse1;
    }
}