#region

using UnityEngine;

#endregion

namespace HealthSystem
{
    public class BurstDamageMarker : MonoBehaviour
    {
        private int _damageAmount;

        #region External Functions

        public void SetDamageAmount(int damageAmount) => _damageAmount = damageAmount;

        public void ApplyDamage()
        {
            var healthAndDamage = GetComponent<HealthAndDamage>();
            healthAndDamage.TakeDamage(_damageAmount);
        }

        #endregion External Functions
    }
}