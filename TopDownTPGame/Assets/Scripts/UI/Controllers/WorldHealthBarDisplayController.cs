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
        [Header("World Display")]
        [SerializeField] private Vector3 _healthBarOffset;

        private Camera _mainCamera;
        private HealthAndDamage _healthAndDamage;
        private int _healthBarId;

        #region Unity Functions

        private void Start()
        {
            _mainCamera = Camera.main;
            _healthAndDamage = GetComponent<HealthAndDamage>();

            var screenPoint = _mainCamera.WorldToScreenPoint(transform.position + _healthBarOffset);
            var mappedPoint = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            var currentHealth = _healthAndDamage.CurrentHealth;
            var maxHealth = _healthAndDamage.MaxHealth;

            _healthBarId = HUD_WorldHealthBarDisplay.Instance.CreateHealthBar(mappedPoint, currentHealth, maxHealth);
        }

        private void OnDestroy() => HUD_WorldHealthBarDisplay.Instance.DestroyHealthBar(_healthBarId);

        private void LateUpdate() => UpdateWorldHealthBar();

        #endregion Unity Functions

        #region Utils

        private void UpdateWorldHealthBar()
        {
            var screenPoint = _mainCamera.WorldToScreenPoint(transform.position + _healthBarOffset);
            var mappedPoint = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            var currentHealth = _healthAndDamage.CurrentHealth;
            var maxHealth = _healthAndDamage.MaxHealth;
            HUD_WorldHealthBarDisplay.Instance.UpdateHealthBar(_healthBarId, mappedPoint, currentHealth, maxHealth);
        }

        #endregion Utils
    }
}