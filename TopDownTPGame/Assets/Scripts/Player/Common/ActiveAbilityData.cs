using Utils.Input;

namespace Player.Common
{
    public struct ActiveAbilityData
    {
        public Ability ability { get; set; }
        public PlayerInputKey abilityKey { get; set; }
        public bool isMovement { get; set; }

        public bool IsValid() => ability != null;

        public void Clear()
        {
            ability = null;
            abilityKey = new PlayerInputKey();
            isMovement = false;
        }
    }
}