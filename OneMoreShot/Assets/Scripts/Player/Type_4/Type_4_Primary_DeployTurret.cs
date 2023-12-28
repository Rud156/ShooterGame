using Player.Abilities;
using Player.Core;

namespace Player.Type_4
{
    public class Type_4_Primary_DeployTurret : AbilityBase
    {
        #region Core Ability Functions

        public override void AbilityFixedUpdate(PlayerController playerController, float fixedDeltaTime)
        {
        }

        public override void AbilityUpdate(PlayerController playerController, float deltaTime)
        {
        }

        public override void AbilityEnd(PlayerController playerController)
        {
        }

        #endregion Core Ability Functions

        #region Ability Conditions

        public override bool AbilityNeedsToEnd(PlayerController playerController)
        {
            return false;
        }

        #endregion Ability Conditions
    }
}