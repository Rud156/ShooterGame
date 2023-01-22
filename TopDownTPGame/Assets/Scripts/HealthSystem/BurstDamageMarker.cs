#region

using UnityEngine;

#endregion

namespace HealthSystem
{
    public class BurstDamageMarker : MonoBehaviour
    {
        private int _damageAmount;
        private int _ownerId;

        #region External Functions

        public void SetDamageAmount(int damageAmount) => _damageAmount = damageAmount;

        public void ApplyDamage()
        {
            var healthAndDamage = GetComponent<HealthAndDamage>();
            healthAndDamage.TakeDamage(_damageAmount);
        }

        public void SetOwner(int ownerId) => _ownerId = ownerId;

        public int GetOwner() => _ownerId;

        #endregion External Functions
    }
}