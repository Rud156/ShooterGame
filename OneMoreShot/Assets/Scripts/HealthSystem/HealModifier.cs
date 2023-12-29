﻿using HeallthSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HealthSystem
{
    [RequireComponent(typeof(HealthAndDamage))]
    public abstract class HealModifier : MonoBehaviour
    {
        private HealthAndDamage _healthAndDamage;
        private string _id;

        #region Unity Functions

        private void Start()
        {
            _id = Guid.NewGuid().ToString();
            _healthAndDamage = GetComponent<HealthAndDamage>();

            _healthAndDamage.AddHealModifier(this, _id);
            HealModifierStart();
        }

        #endregion Unity Functions

        public abstract void HealModifierStart();

        public abstract void HealModifierUpdate();

        public abstract bool HealModifierNeedsToEnd();

        public virtual void HealModifierEnd() => _healthAndDamage.RemoveHealModifier(_id);

        public abstract int ModifiedHeal(int inputHealAmount);
    }
}