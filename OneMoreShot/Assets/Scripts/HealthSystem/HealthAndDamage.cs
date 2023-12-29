using EditorCools;
using HealthSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeallthSystem
{
    public class HealthAndDamage : MonoBehaviour
    {
        [Header("Health Data")]
        [SerializeField] private int _maxHealth;

        [Header("Debug")]
        [SerializeField] private int _debugDamageAmount;
        [SerializeField] private int _debugHealAmount;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;

        private int _currentHealth;
        private List<Tuple<string, HealModifier>> _healModifiers;
        private List<Tuple<string, DamageModifier>> _damageModifiers;

        public delegate void HealthChanged(int startHealth, int currentHealth, int maxHealth);
        public delegate void DamageTaken(int damageTaken, int startingHealth, int finalHealth);
        public delegate void Healed(int healAmount, int startingHealth, int finalHealth);
        public delegate void HealModifierAdded(HealModifier healModifier, int currentHealth);
        public delegate void HealModifierRemoved(HealModifier healModifier, int currentHealth);
        public delegate void DamageModifierAdded(DamageModifier damageModifier, int currentHealth);
        public delegate void DamageModifierRemoved(DamageModifier damageModifier, int currentHealth);

        public event HealthChanged OnHealthChanged;
        public event DamageTaken OnDamageTaken;
        public event Healed OnHealed;
        public event HealModifierAdded OnHealModifierAdded;
        public event HealModifierRemoved OnHealModifierRemoved;
        public event DamageModifierAdded OnDamageModifierAdded;
        public event DamageModifierRemoved OnDamageModifierRemoved;

        #region Unity Functions

        private void Start()
        {
            _healModifiers = new List<Tuple<string, HealModifier>>();
            _damageModifiers = new List<Tuple<string, DamageModifier>>();
            _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _currentHealth, _maxHealth);
        }

        private void Update()
        {
            UpdateHealModifiers();
            UpdateDamageModifiers();
        }

        #endregion Unity Functions

        #region Heal Changes

        public void TakeHeal(int healAmount)
        {
            var startHealth = _currentHealth;
            var modifiedHealAmount = healAmount;
            foreach (var healModifier in _healModifiers)
            {
                modifiedHealAmount = healModifier.Item2.ModifiedHeal(modifiedHealAmount);
            }

            _currentHealth += modifiedHealAmount;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

            OnHealed?.Invoke(modifiedHealAmount, startHealth, _currentHealth);
            OnHealthChanged?.Invoke(startHealth, _currentHealth, _maxHealth);
        }

        public void AddHealModifier(HealModifier healModifier, string id)
        {
            _healModifiers.Add(new Tuple<string, HealModifier>(id, healModifier));
            OnHealModifierAdded?.Invoke(healModifier, _currentHealth);
        }

        public void RemoveHealModifier(string id)
        {
            var index = GetHealModifierIndex(id);
            if (index != -1)
            {
                var healModifier = _healModifiers[index].Item2;
                OnHealModifierRemoved?.Invoke(healModifier, _currentHealth);
                _healModifiers.RemoveAt(index);
            }
        }

        private void UpdateHealModifiers()
        {
            for (var i = _healModifiers.Count - 1; i >= 0; i--)
            {
                _healModifiers[i].Item2.HealModifierUpdate();
                if (_healModifiers[i].Item2.HealModifierNeedsToEnd())
                {
                    _healModifiers[i].Item2.HealModifierEnd();
                }
            }
        }

        private int GetHealModifierIndex(string id)
        {
            for (var i = 0; i < _healModifiers.Count; i++)
            {
                if (_healModifiers[i].Item1 == id)
                {
                    return i;
                }
            }

            return -1;
        }

        #endregion Heal Changes

        #region Damage Changes

        private void TakeDamage(int damageAmount)
        {
            var startHealth = _currentHealth;
            var modifiedDamageAmount = damageAmount;
            foreach (var damageModifier in _damageModifiers)
            {
                modifiedDamageAmount = damageModifier.Item2.ModifiedDamage(modifiedDamageAmount);
            }

            _currentHealth -= modifiedDamageAmount;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

            OnDamageTaken?.Invoke(modifiedDamageAmount, startHealth, _currentHealth);
            OnHealthChanged?.Invoke(startHealth, _currentHealth, _maxHealth);
        }

        public void AddDamageModifier(DamageModifier damageModifier, string id)
        {
            _damageModifiers.Add(new Tuple<string, DamageModifier>(id, damageModifier));
            OnDamageModifierAdded?.Invoke(damageModifier, _currentHealth);
        }

        public void RemoveDamageModifier(string id)
        {
            var index = GetDamageModifierIndex(id);
            if (index != -1)
            {
                var damageModifier = _damageModifiers[index].Item2;
                OnDamageModifierRemoved?.Invoke(damageModifier, _currentHealth);
                _damageModifiers.RemoveAt(index);
            }
        }

        private void UpdateDamageModifiers()
        {
            for (var i = _damageModifiers.Count - 1; i >= 0; i--)
            {
                _damageModifiers[i].Item2.DamageModifierUpdate();
                if (_damageModifiers[i].Item2.DamageModifierNeedsToEnd())
                {
                    _damageModifiers[i].Item2.DamageModifierEnd();
                }
            }
        }

        private int GetDamageModifierIndex(string id)
        {
            for (var i = 0; i < _damageModifiers.Count; i++)
            {
                if (_damageModifiers[i].Item1 == id)
                {
                    return i;
                }
            }

            return -1;
        }

        #endregion Damage Changes

        #region Debug

        [Button]
        public void DebugTakeHeal() => TakeHeal(_debugHealAmount);

        [Button]
        public void DebugTakeDamage() => TakeDamage(_debugDamageAmount);

        #endregion Debug
    }
}