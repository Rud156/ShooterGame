using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Misc;

namespace UI.Player
{
    public class HUD_PlayerHealthDisplay : MonoBehaviour
    {
        private const string PlayerHealthBarParentString = "UI_PlayerHealthWidget";
        private const string PlayerHealthBarString = "PlayerHealthBar";
        private const string CurrentHealthString = "CurrentHealthLabel";
        private const string MaxHealthString = "MaxHealthLabel";

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

        [Header("Bar Scaler")]
        [SerializeField] private float _defaultScale;
        [SerializeField] private float _biggerScale;
        [SerializeField] private int _scaleCount;
        [SerializeField] private float _scaleChangeDuration;

        private VisualElement _parent;

        private VisualElement _progressBarProgress;
        private VisualElement _progressBar;
        private ProgressBar _healthBar;
        private Label _currentHealthLabel;
        private Label _maxHealthLabel;

        private float _startHealthLerp;
        private float _targetHealthLerp;
        private float _lerpAmount;

        // Coroutine Data
        private bool _flashCoroutineActive;
        private bool _scaleCoroutineActive;

        #region Unity Functions

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

        #endregion

        #region External Functions

        public void DisplayHealthChanged(int currentHealth, int maxHealth)
        {
            _currentHealthLabel.text = currentHealth.ToString();
            _maxHealthLabel.text = $"/ {maxHealth}";

            _healthBar.lowValue = 0;
            _healthBar.highValue = maxHealth;

            _startHealthLerp = _healthBar.value;
            _targetHealthLerp = currentHealth;
            _lerpAmount = 0;

            var healthRatio = (float)currentHealth / maxHealth;
            var healthColor = healthRatio <= 0.5
                ? Color.Lerp(_lowHealthColor, _midHealthColor, healthRatio * 2)
                : Color.Lerp(_midHealthColor, _fullHealthColor, (healthRatio - 0.5f) * 2);
            if (!_flashCoroutineActive)
            {
                StartCoroutine(BarFlasher(healthColor));
            }

            if (!_scaleCoroutineActive)
            {
                StartCoroutine(BarScaler());
            }
        }

        #endregion

        #region Bar Effects

        private IEnumerator BarFlasher(Color finalColor)
        {
            _flashCoroutineActive = true;
            var startColor = _progressBarProgress.style.backgroundColor.value;
            for (var i = 0; i < _flashCount; i++)
            {
                _progressBarProgress.style.backgroundColor = _flashColor;
                yield return new WaitForSeconds(_flashOnDuration);
                _progressBarProgress.style.backgroundColor = startColor;
                yield return new WaitForSeconds(_flashOffDuration);
            }

            _progressBarProgress.style.backgroundColor = finalColor;
            _flashCoroutineActive = false;
        }

        private IEnumerator BarScaler()
        {
            _scaleCoroutineActive = true;
            for (var i = 0; i < _scaleCount; i++)
            {
                _progressBar.style.scale = Vector2.one * _biggerScale;
                yield return new WaitForSeconds(_scaleChangeDuration);
                _progressBar.style.scale = Vector2.one * _defaultScale;
                yield return new WaitForSeconds(_scaleChangeDuration);
            }

            _scaleCoroutineActive = false;
        }

        #endregion

        #region Setup

        private void Initialize()
        {
            var root = GameObject.FindWithTag(TagManager.UIRoot).GetComponent<UIDocument>().rootVisualElement;
            _parent = root.Q<VisualElement>(PlayerHealthBarParentString);
            _healthBar = _parent.Q<ProgressBar>(PlayerHealthBarString);
            _currentHealthLabel = _parent.Q<Label>(CurrentHealthString);
            _maxHealthLabel = _parent.Q<Label>(MaxHealthString);

            _progressBar = _healthBar.Q<VisualElement>("unity-progress-bar");
            var progressBackground = _progressBar.Q<VisualElement>(className: "unity-progress-bar__background");
            _progressBarProgress = progressBackground.Q<VisualElement>(className: "unity-progress-bar__progress");
        }

        #endregion

        #region Singleton

        public static HUD_PlayerHealthDisplay Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Initialize();
        }

        #endregion Singleton
    }
}