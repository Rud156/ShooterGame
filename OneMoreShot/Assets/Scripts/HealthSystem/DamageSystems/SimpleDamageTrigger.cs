using HeallthSystem;
using System;
using UnityEngine;
using Utils.Common;

namespace HealthSystem.DamageSystems
{
    public class SimpleDamageTrigger : MonoBehaviour
    {
        [Header("Damage Data")]
        [SerializeField] private int _damageAmount;

        private Action<Collider> _callbackFunc;

        #region Unity Functions

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<OwnerData>(out var otherOwnerData))
            {
                _callbackFunc?.Invoke(other);
                return;
            }

            if (otherOwnerData.OwnerId != GetComponent<OwnerData>().OwnerId && other.TryGetComponent(out HealthAndDamage healthAndDamage))
            {
                healthAndDamage.TakeDamage(_damageAmount);
                _callbackFunc?.Invoke(other);
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void SetCollisionCallback(Action<Collider> callback) => _callbackFunc = callback;

        #endregion External Functions
    }
}