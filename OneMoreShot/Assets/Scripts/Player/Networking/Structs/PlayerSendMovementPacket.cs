using UnityEngine;
using Utils.Input;

namespace Player.Networking.Structs
{
    public struct PlayerSendMovementPacket
    {
        public float TimeStamp;

        public Vector2 CoreInput;
        public Vector2 MousePosition;

        public PlayerInputKey JumpInput;
        public PlayerInputKey AbilityPrimaryKey;
        public PlayerInputKey AbilitySecondaryKey;
    }
}