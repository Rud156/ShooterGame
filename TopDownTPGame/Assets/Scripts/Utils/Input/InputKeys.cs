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

        // Ability Keys
        public const KeyCode AbilityPrimary = KeyCode.Mouse0;
        public const KeyCode AbilitySecondary = KeyCode.Mouse1;
        public const KeyCode AbilityTertiary = KeyCode.Mouse3;
        public const KeyCode AbilityTertiarySecondary = KeyCode.F;
        public const KeyCode AbilityUltimate = KeyCode.X;

        // Secondary Movement Keys
        public const KeyCode MovementHoldInput = KeyCode.Space;
    }
}