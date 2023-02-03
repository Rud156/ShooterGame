#region

using Player.Base;
using Player.Common;

#endregion

namespace Player.Type_4
{
    public class Type_4_Tertiary_SatchelMovement : Ability
    {
        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => true;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
        }

        public override void EndAbility(BasePlayerController playerController)
        {
        }

        public override void StartAbility(BasePlayerController playerController)
        {
        }

        #endregion Ability Functions
    }
}