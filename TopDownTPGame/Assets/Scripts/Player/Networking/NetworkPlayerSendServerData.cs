namespace Player.Networking
{
    public struct NetworkPlayerSendServerData
    {
        // WASD Movement
        public int XInput;
        public int YInput;

        // Rotation
        public int XRotation;
        public int YRotation;

        // Input Type
        public int PlayerInputType;

        // Core Movement
        public NetworkPlayerInputKey JumpKey;
        public NetworkPlayerInputKey RunKey;

        // Ability Keys
        public NetworkPlayerInputKey AbilityPrimaryKey;
        public NetworkPlayerInputKey AbilitySecondaryKey;
        public NetworkPlayerInputKey AbilityTertiaryKey;
        public NetworkPlayerInputKey AbilityUltimateKey;
        public NetworkPlayerInputKey ConstantSpeedFallKey;

        // Package Data
        public uint InputSequenceCount;
        public float TimeStamp;
    }
}