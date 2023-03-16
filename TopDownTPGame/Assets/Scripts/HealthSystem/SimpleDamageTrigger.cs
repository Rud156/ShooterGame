#region

using System;
using UnityEngine;

#endregion

namespace HealthSystem
{
    public class SimpleDamageTrigger : MonoBehaviour
    {
        [Header("Parent")]
        [SerializeField] private GameObject _parent;

        [Header("Damage Data")]
        [SerializeField] private int _damageAmount;

        private Action<Collider> _callbackFunc;

        #region Unity Functions

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetInstanceID() != _parent.GetInstanceID() && other.TryGetComponent(out HealthAndDamage healthAndDamage))
            {
                healthAndDamage.TakeDamage(_damageAmount);
                _callbackFunc?.Invoke(other);
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void SetCollisionCallback(Action<Collider> callback) => _callbackFunc = callback;

        public void SetParent(GameObject parent) => _parent = parent;

        #endregion External Functions
    }
}