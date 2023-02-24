#region

using UnityEngine;

#endregion

namespace Player.Base
{
    public class ResetJumpIntegerOnState : StateMachineBehaviour
    {
        private static readonly int FallJumpTriggerParam = Animator.StringToHash("FallingAndJumpTrigger");

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) => animator.SetInteger(FallJumpTriggerParam, 0);
    }
}