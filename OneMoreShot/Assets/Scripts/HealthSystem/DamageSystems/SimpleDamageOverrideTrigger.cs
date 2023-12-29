using HeallthSystem;
using System;
using UnityEngine;

namespace HealthSystem.DamageSystems
{
    public class SimpleDamageOverrideTrigger : MonoBehaviour
    {
        private int _damageAmount;
        private Action<Collider> _callbackFunc;

        #region Unity Functions

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out HealthAndDamage healthAndDamage))
            {
                healthAndDamage.TakeDamage(_damageAmount);
                _callbackFunc?.Invoke(other);
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void SetDamageAmount(int damageAmount) => _damageAmount = damageAmount;

        public void SetCollisionCallback(Action<Collider> callback) => _callbackFunc = callback;

        #endregion External Functions
    }
}