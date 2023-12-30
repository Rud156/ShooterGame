namespace Player.Core
{
    public enum PlayerState
    {
        Idle,
        Running,
        Falling,
        Dead,

        FrozenMovementInput, // This is to lock position and rotation when attacking
        CustomMovement, // This is used for Ability Controlled Movements
    }
}