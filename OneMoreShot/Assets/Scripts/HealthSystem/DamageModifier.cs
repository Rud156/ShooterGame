using HeallthSystem;
using System;
using UnityEngine;

namespace HealthSystem
{
    [RequireComponent(typeof(HealthAndDamage))]
    public abstract class DamageModifier : MonoBehaviour
    {
        private HealthAndDamage _healthAndDamage;
        private string _id;

        #region Unity Functions

        private void Start()
        {
            _id = Guid.NewGuid().ToString();
            _healthAndDamage = GetComponent<HealthAndDamage>();

            _healthAndDamage.AddDamageModifier(this, _id);
            DamageModifierStart();
        }

        #endregion Unity Functions

        public abstract void DamageModifierStart();

        public abstract void DamageModifierUpdate();

        public abstract bool DamageModifierNeedsToEnd();

        public virtual void DamageModifierEnd() => _healthAndDamage.RemoveDamageModifier(_id);

        public abstract int ModifiedDamage(int inputDamageAmount);
    }
}