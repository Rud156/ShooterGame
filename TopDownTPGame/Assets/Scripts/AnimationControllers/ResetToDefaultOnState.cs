#region

using UnityEngine;

#endregion

namespace Player.AnimationControllers
{
    public class ResetToDefaultOnState : StateMachineBehaviour
    {
        [SerializeField] private string _paramName;
        [SerializeField] private int _defaultValue;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) => animator.SetInteger(_paramName, _defaultValue);
    }
}