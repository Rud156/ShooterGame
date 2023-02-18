#region

using UnityEngine;

#endregion

namespace HealthSystem
{
    public class SimpleDamageOverrideTrigger : MonoBehaviour
    {
        private int _damageAmount;

        #region Unity Functions

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out HealthAndDamage healthAndDamage))
            {
                healthAndDamage.TakeDamage(_damageAmount);
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void SetDamageAmount(int damageAmount) => _damageAmount = damageAmount;

        #endregion External Functions
    }
}