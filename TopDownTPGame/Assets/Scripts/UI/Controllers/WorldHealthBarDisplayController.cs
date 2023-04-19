#region

using HealthSystem;
using UI.DisplayManagers.HUD;
using UnityEngine;

#endregion

namespace UI.Controllers
{
    [RequireComponent(typeof(HealthAndDamage))]
    public class WorldHealthBarDisplayController : MonoBehaviour
    {
        private HealthAndDamage _healthAndDamage;
        private int _healthBarId;

        #region Unity Functions

        private void Start()
        {
            _healthAndDamage = GetComponent<HealthAndDamage>();

            var position = Vector2.zero;
            var currentHealth = _healthAndDamage.CurrentHealth;
            var maxHealth = _healthAndDamage.MaxHealth;

            _healthBarId = HUD_WorldHealthBarDisplay.Instance.CreateHealthBar(position, currentHealth, maxHealth);
            _healthAndDamage.OnHealthChanged += HandleHealthChanged;
        }

        private void OnDestroy()
        {
            HUD_WorldHealthBarDisplay.Instance.DestroyHealthBar(_healthBarId);
            _healthAndDamage.OnHealthChanged -= HandleHealthChanged;
        }

        #endregion Unity Functions

        #region Utils

        private void HandleHealthChanged(int startHealth, int currentHealth, int maxHealth)
        {
            var position = Vector2.zero;
            HUD_WorldHealthBarDisplay.Instance.UpdateHealthBar(_healthBarId, position, currentHealth, maxHealth);
        }

        #endregion Utils
    }
}