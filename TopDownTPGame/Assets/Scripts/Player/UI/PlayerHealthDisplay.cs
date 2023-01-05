#region

using EditorCools;
using HealthSystem;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Utils.Misc;

#endregion

namespace UI
{
    [RequireComponent(typeof(HealthAndDamage))]
    public class PlayerHealthDisplay : MonoBehaviour
    {
        private const string PlayerHealthBar = "PlayerHealth";
        private const string CurrentHealthString = "CurrentHealth";
        private const string MaxHealthString = "MaxHealth";

        [Header("Display Colors")]
        [SerializeField] private Color _lowHealthColor;
        [SerializeField] private Color _midHealthColor;
        [SerializeField] private Color _fullHealthColor;

        private HealthAndDamage _healthAndDamage;
        private VisualElement _root;

        private VisualElement _progressBarProgress;
        private ProgressBar _healthBar;
        private Label _currentHealthLabel;
        private Label _maxHealthLabel;

        #region Unity Functions

        private void Start()
        {
            _healthAndDamage = GetComponent<HealthAndDamage>();
            _root = GameObject.FindWithTag(TagManager.UIRoot).GetComponent<UIDocument>().rootVisualElement;
            _healthBar = _root.Q<ProgressBar>(PlayerHealthBar);
            _currentHealthLabel = _root.Q<Label>(CurrentHealthString);
            _maxHealthLabel = _root.Q<Label>(MaxHealthString);

            var progressBar = _healthBar.Q<VisualElement>("unity-progress-bar");
            var progressBackground = progressBar.Q<VisualElement>(className: "unity-progress-bar__background");
            _progressBarProgress = progressBackground.Q<VisualElement>(className: "unity-progress-bar__progress");

            var titleContainer = progressBackground.Q<VisualElement>(className: "unity-progress-bar__title-container");
            var innerText = titleContainer.Q<Label>(className: "unity-progress-bar__title");
            innerText.text = string.Empty;

            _healthAndDamage.OnHealthChanged += HandleHealthChanged;
        }

        private void OnDestroy() => _healthAndDamage.OnHealthChanged -= HandleHealthChanged;

        #endregion Unity Functions

        #region Health Events

        private void HandleHealthChanged(int startingHealth, int currentHealth, int maxHealth)
        {
            _currentHealthLabel.text = currentHealth.ToString();
            _maxHealthLabel.text = maxHealth.ToString();

            _healthBar.lowValue = 0;
            _healthBar.highValue = maxHealth;
            _healthBar.value = currentHealth;

            var healthRatio = (float)currentHealth / maxHealth;
            var healthColor = healthRatio <= 0.5
                ? Color.Lerp(_lowHealthColor, _midHealthColor, healthRatio * 2)
                : Color.Lerp(_midHealthColor, _fullHealthColor, (healthRatio - 0.5f) * 2);
            _progressBarProgress.style.unityBackgroundImageTintColor = healthColor;
        }

        #endregion Health Events

        #region Debug

        [Button]
        public void DebugTakeDamage() => _healthAndDamage.TakeDamage(10);

        [Button]
        public void DebugTakeHeal() => _healthAndDamage.TakeHeal(10);

        #endregion Debug
    }
}