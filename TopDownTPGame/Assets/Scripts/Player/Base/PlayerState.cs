namespace Player.Base
{
    public enum PlayerState
    {
        Idle,
        Walking,
        Running,
        Falling,

        Custom,

        // The states below these are custom states that prevent any normal input
        // These should not be use for any normal gameplay
        Frozen,
    };
}