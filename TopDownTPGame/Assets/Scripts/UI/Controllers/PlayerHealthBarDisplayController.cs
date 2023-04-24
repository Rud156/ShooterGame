#region

using HealthSystem;
using UI.DisplayManagers.Player;
using UnityEngine;

#endregion

namespace UI.Controllers
{
    [RequireComponent(typeof(HealthAndDamage))]
    public class PlayerHealthBarDisplayController : MonoBehaviour
    {
        [Header("Components")]
        private HealthAndDamage _healthAndDamage;

        #region Unity Functions

        private void Start()
        {
            _healthAndDamage = GetComponent<HealthAndDamage>();
            _healthAndDamage.OnHealthChanged += DisplayHealthChangeToHUD;
        }

        private void OnDestroy() => _healthAndDamage.OnHealthChanged -= DisplayHealthChangeToHUD;

        #endregion Unity Functions

        #region Utils

        private void DisplayHealthChangeToHUD(int startHealth, int currentHealth, int maxHealth) => HUD_PlayerHealthDisplay.Instance.DisplayHealthChanged(currentHealth, maxHealth);

        #endregion Utils
    }
}