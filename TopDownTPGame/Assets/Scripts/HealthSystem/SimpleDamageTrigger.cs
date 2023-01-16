#region

using UnityEngine;

#endregion

namespace HealthSystem
{
    public class SimpleDamageTrigger : MonoBehaviour
    {
        [Header("Damage Data")]
        [SerializeField] private int _damageAmount;

        #region Unity Functions

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out HealthAndDamage healthAndDamage))
            {
                healthAndDamage.TakeDamage(_damageAmount);
            }
        }

        #endregion Unity Functions
    }
}