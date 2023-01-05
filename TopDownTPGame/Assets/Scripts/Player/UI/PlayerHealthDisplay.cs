#region

using System.Collections;
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

        [Header("Lerp Data")]
        [SerializeField] private float _displayLerpSpeed;
        [SerializeField] private AnimationCurve _lerrpCurve;

        [Header("Display Colors")]
        [SerializeField] private Color _lowHealthColor;
        [SerializeField] private Color _midHealthColor;
        [SerializeField] private Color _fullHealthColor;

        [Header("Bar Flash")]
        [SerializeField] private Color _flashColor;
        [SerializeField] private int _flashCount;
        [SerializeField] private float _flashOnDuration;
        [SerializeField] private float _flashOffDuration;

        [Header("Debug")]
        [SerializeField] private int _debugDamageAmount;
        [SerializeField] private int _debugHealAmount;

        private HealthAndDamage _healthAndDamage;
        private VisualElement _root;

        private VisualElement _progressBarProgress;
        private ProgressBar _healthBar;
        private Label _currentHealthLabel;
        private Label _maxHealthLabel;

        private float _startHealthLerp;
        private float _targetHealthLerp;
        private float _lerpAmount;

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
            innerText.style.display = DisplayStyle.None;

            _healthAndDamage.OnHealthChanged += HandleHealthChanged;
        }

        private void OnDestroy() => _healthAndDamage.OnHealthChanged -= HandleHealthChanged;

        private void Update()
        {
            if (_lerpAmount > 1)
            {
                return;
            }

            var percent = _lerrpCurve.Evaluate(_lerpAmount);
            var mappedValue = Mathf.Lerp(_startHealthLerp, _targetHealthLerp, percent);
            _healthBar.value = mappedValue;
            _lerpAmount += Time.deltaTime * _displayLerpSpeed;
        }

        #endregion Unity Functions

        #region Health Events

        private void HandleHealthChanged(int startingHealth, int currentHealth, int maxHealth)
        {
            _currentHealthLabel.text = currentHealth.ToString();
            _maxHealthLabel.text = maxHealth.ToString();

            _healthBar.lowValue = 0;
            _healthBar.highValue = maxHealth;

            _startHealthLerp = _healthBar.value;
            _targetHealthLerp = currentHealth;
            _lerpAmount = 0;

            var healthRatio = (float)currentHealth / maxHealth;
            var healthColor = healthRatio <= 0.5
                ? Color.Lerp(_lowHealthColor, _midHealthColor, healthRatio * 2)
                : Color.Lerp(_midHealthColor, _fullHealthColor, (healthRatio - 0.5f) * 2);
            StartCoroutine(BarFlasher(healthColor));
        }

        private IEnumerator BarFlasher(Color finalColor)
        {
            var startColor = _progressBarProgress.style.backgroundColor.value;
            for (var i = 0; i < _flashCount; i++)
            {
                _progressBarProgress.style.backgroundColor = _flashColor;
                yield return new WaitForSeconds(_flashOnDuration);
                _progressBarProgress.style.backgroundColor = startColor;
                yield return new WaitForSeconds(_flashOffDuration);
            }

            _progressBarProgress.style.backgroundColor = finalColor;
        }

        #endregion Health Events

        #region Debug

        [Button]
        public void DebugTakeDamage() => _healthAndDamage.TakeDamage(_debugDamageAmount);

        [Button]
        public void DebugTakeHeal() => _healthAndDamage.TakeHeal(_debugHealAmount);

        #endregion Debug
    }
}