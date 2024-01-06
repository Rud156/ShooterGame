using UnityEngine;
using Utils.Input;

namespace Player.Networking.Structs
{
    public class PlayerSendMovementPacket : CoreNetworkingPacket
    {
        public Vector2 CoreInput;
        public Vector2 MousePosition;

        public PlayerInputKey JumpInput;
        public PlayerInputKey AbilityPrimaryKey;
        public PlayerInputKey AbilitySecondaryKey;

        public PlayerSendMovementPacket()
        {
            CoreInput = Vector2.zero;
            MousePosition = Vector2.zero;
            JumpInput = new PlayerInputKey();
            AbilityPrimaryKey = new PlayerInputKey();
            AbilitySecondaryKey = new PlayerInputKey();
        }

        public PlayerSendMovementPacket(PlayerSendMovementPacket other)
        {
            CoreInput = other.CoreInput;
            MousePosition = other.MousePosition;
            JumpInput = new PlayerInputKey(other.JumpInput);
            AbilityPrimaryKey = new PlayerInputKey(other.AbilityPrimaryKey);
            AbilitySecondaryKey = new PlayerInputKey(other.AbilitySecondaryKey);
        }
    }
}