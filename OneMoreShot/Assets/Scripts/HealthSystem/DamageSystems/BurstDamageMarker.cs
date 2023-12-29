using HeallthSystem;
using UnityEngine;

namespace HealthSystem.DamageSystems
{
    public class BurstDamageMarker : MonoBehaviour
    {
        private int _damageAmount;

        #region External Functions

        public void SetDamageAmount(int damageAmount) => _damageAmount = damageAmount;

        public void ApplyDamage(bool useParent)
        {
            var targetGameObject = useParent ? transform.parent.gameObject : transform.gameObject;
            var healthAndDamage = targetGameObject.GetComponent<HealthAndDamage>();

            healthAndDamage.TakeDamage(_damageAmount);
        }

        #endregion External Functions
    }
}