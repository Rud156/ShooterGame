#region

using System;
using System.Collections.Generic;
using EditorCools;
using UnityEngine;

#endregion

namespace HealthSystem
{
    public class HealthAndDamage : MonoBehaviour
    {
        [Header("Health Data")]
        [SerializeField] private int _maxHealth;

        [Header("Auto Heal Data")]
        [SerializeField] private AutoHealWhen _autoHealWhen;
        [SerializeField] private float _autoHealTickRate;
        [SerializeField] private int _autoHealPerTickAmount;

        [Header("Debug")]
        [SerializeField] private int _debugDamageAmount;
        [SerializeField] private int _debugHealAmount;

        private int _currentHealth;
        private List<Tuple<string, HealModifier>> _healModifiers;
        private List<Tuple<string, DamageModifier>> _damageModifiers;

        private AutoHealWhen _currentAutoHealState;
        private float _autoHealTimer;

        public delegate void HealthChanged(int startHealth, int currentHealth, int maxHealth);
        public delegate void DamageTaken(int damageTaken, int startingHealth, int finalHealth);
        public delegate void Healed(int healAmount, int startingHealth, int finalHealth);
        public delegate void HealModifierAdded(HealModifier healModifier, int currentHealth);
        public delegate void HealModifierRemoved(HealModifier healModifier, int currentHealth);
        public delegate void DamageModifierAdded(DamageModifier damageModifier, int currentHealth);
        public delegate void DamageModifierRemoved(DamageModifier damageModifier, int currentHealth);


        public HealthChanged OnHealthChanged;
        public DamageTaken OnDamageTaken;
        public Healed OnHealed;
        public HealModifierAdded OnHealModifierAdded;
        public HealModifierRemoved OnHealModifierRemoved;
        public DamageModifierAdded OnDamageModifierAdded;
        public DamageModifierRemoved OnDamageModifierRemoved;

        #region Unity Functions

        private void Start()
        {
            _healModifiers = new List<Tuple<string, HealModifier>>();
            _damageModifiers = new List<Tuple<string, DamageModifier>>();
            _currentHealth = _maxHealth;

            SetAutoHealState(_autoHealWhen);
            OnHealthChanged?.Invoke(_currentHealth, _currentHealth, _maxHealth);
        }

        private void FixedUpdate()
        {
            UpdateAutoHeal();
            UpdateHealModifiers();
            UpdateDamageModifiers();
        }

        #endregion Unity Functions

        #region Auto Heal

        private void UpdateAutoHeal()
        {
            switch (_currentAutoHealState)
            {
                case AutoHealWhen.NoAutoHeal:
                    // Don't do anything here...
                    break;

                case AutoHealWhen.Always:
                {
                    _autoHealTimer -= Time.fixedDeltaTime;
                    if (_autoHealTimer <= 0)
                    {
                        TakeHeal(_autoHealPerTickAmount);
                        _autoHealTimer = _autoHealTickRate;
                    }
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetAutoHealState(AutoHealWhen autoHealWhen) => _currentAutoHealState = autoHealWhen;

        #endregion Auto Heal

        #region Modifiers Update

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

        #endregion Modifiers Update

        #region Damage And Heal

        public void TakeDamage(int damageAmount)
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

        #endregion Damage And Heal

        #region Utils

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

        #endregion Utils

        #region Debug

        [Button]
        public void DebugTakeDamage() => TakeDamage(_debugDamageAmount);

        [Button]
        public void DebugTakeHeal() => TakeHeal(_debugHealAmount);

        #endregion Debug

        #region Enums

        public enum AutoHealWhen
        {
            NoAutoHeal,
            Always,
        }

        #endregion Enums
    }
}