using UnityEngine;
using Random = UnityEngine.Random;

namespace Animations
{
    public class RandomAnimSelector : StateMachineBehaviour
    {
        [SerializeField] private int m_minAnimIndex;
        [SerializeField] private int m_maxAnimIndex;
        [SerializeField] private string m_animVariableName;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            int randomIndex = Random.Range(m_minAnimIndex, m_maxAnimIndex + 1);
            animator.SetFloat(m_animVariableName, randomIndex);

        }
    }
}