using UnityEngine;
using Utils.Input;

namespace Player.Networking.Structs
{
    [System.Serializable]
    public class PlayerSendMovementPacket : CoreNetworkingPacket
    {
        public Vector2 RawInput;
        public Vector2 CoreInput;
        public Vector2 LastNonZeroInput;
        public Vector2 MousePosition;

        public PlayerInputKey JumpInput;
        public PlayerInputKey AbilityPrimaryKey;
        public PlayerInputKey AbilitySecondaryKey;

        public PlayerSendMovementPacket()
        {
            RawInput = Vector2.zero;
            CoreInput = Vector2.zero;
            LastNonZeroInput = Vector2.zero;
            MousePosition = Vector2.zero;
            JumpInput = new PlayerInputKey();
            AbilityPrimaryKey = new PlayerInputKey();
            AbilitySecondaryKey = new PlayerInputKey();
        }

        public PlayerSendMovementPacket(PlayerSendMovementPacket other)
        {
            RawInput = other.RawInput;
            CoreInput = other.CoreInput;
            LastNonZeroInput = other.LastNonZeroInput;
            MousePosition = other.MousePosition;
            JumpInput = new PlayerInputKey(other.JumpInput);
            AbilityPrimaryKey = new PlayerInputKey(other.AbilityPrimaryKey);
            AbilitySecondaryKey = new PlayerInputKey(other.AbilitySecondaryKey);
        }
    }
}