using HeallthSystem;
using UnityEngine;

namespace UI.Player
{
    [RequireComponent(typeof(HealthAndDamage))]
    public class HUD_PlayerHealthBarDisplayController : MonoBehaviour
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