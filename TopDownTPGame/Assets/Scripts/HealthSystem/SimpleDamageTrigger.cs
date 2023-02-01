#region

using System;
using UnityEngine;

#endregion

namespace HealthSystem
{
    public class SimpleDamageTrigger : MonoBehaviour
    {
        [Header("Damage Data")]
        [SerializeField] private int _damageAmount;

        private Action<Collider> _callbackFunc;

        #region Unity Functions

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out HealthAndDamage healthAndDamage))
            {
                healthAndDamage.TakeDamage(_damageAmount);
            }

            _callbackFunc?.Invoke(other);
        }

        #endregion Unity Functions

        #region External Functions

        public void SetCollisionCallback(Action<Collider> callback) => _callbackFunc = callback;

        #endregion External Functions
    }
}