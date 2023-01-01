#region

using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_1
{
    public class Type_1_Ulitimate : Ability
    {
        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        // This is a one time ability and so needs to end instantly...
        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => true;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            // Nothing to update here...
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            // Nothing to do here...
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            Debug.Log("Starting Ultimate");
            // TODO: Communicate to a global modifier that timers have changed...
        }
    }
}