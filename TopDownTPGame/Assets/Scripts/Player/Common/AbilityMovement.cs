using UnityEngine;

namespace Player.Common
{
    public abstract class AbilityMovement : MonoBehaviour
    {
        public abstract void StartAbility();

        public abstract Vector3 AbilityMove(Vector3 currentVelocity, Vector3 coreInput);

        public abstract void EndAbility();

        public abstract bool AbilityCanStart();

        public abstract bool AbilityNeedsToEnd();
    }
}