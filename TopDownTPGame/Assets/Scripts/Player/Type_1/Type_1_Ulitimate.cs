using Player.Base;
using Player.Common;
using UnityEngine;

namespace Player.Type_1
{
    public class Type_1_Ulitimate : Ability
    {
        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        // This is a one time ability and so needs to end instantly...
        public override bool AbilityNeedsToEnd() => true;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            // Nothing to update here...
        }

        public override void EndAbility()
        {
            // Nothing to do here...
        }

        public override void StartAbility()
        {
            Debug.Log("Starting Ultimate");
            // TODO: Communicate to a global modifier that timers have changed...
        }
    }
}