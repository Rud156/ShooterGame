namespace Player.Core
{
    public enum PlayerState
    {
        Idle,
        Running,
        Falling,
        Dead,

        CustomMovement, // This is used for Ability Controlled Movements
    }
}