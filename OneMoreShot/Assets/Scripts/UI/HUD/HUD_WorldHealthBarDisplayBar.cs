#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Misc;

#endregion

namespace UI.HUD
{
    public class HUD_WorldHealthBarDisplayBar : MonoBehaviour
    {
        private const string HealthBarParentString = "UI_WorldHealthBarContainer";
        private const string HealthProgressBarString = "HealthProgressBar";

        [Header("Prefabs")]
        [SerializeField] private VisualTreeAsset _healthBarPrefab;

        [Header("Display Colors")]
        [SerializeField] private Color _lowHealthColor;
        [SerializeField] private Color _midHealthColor;
        [SerializeField] private Color _fullHealthColor;

        private VisualElement _parent;
        private Dictionary<int, HealthBarData> _healthBars;
        private int _healthBarCounterId;

        #region Utils

        #region External Functions

        public int CreateHealthBar(Vector2 position, int currentHealth, int maxHealth)
        {
            var healthBarId = _healthBarCounterId;
            _healthBarCounterId += 1;

            var healthBarWidget = _healthBarPrefab.Instantiate();
            var healthProgressBar = healthBarWidget.Q<ProgressBar>(HealthProgressBarString);
            _parent.Add(healthBarWidget);

            var progressBar = healthProgressBar.Q<VisualElement>("unity-progress-bar");
            var progressBackground = progressBar.Q<VisualElement>(className: "unity-progress-bar__background");
            var progressBarProgress = progressBackground.Q<VisualElement>(className: "unity-progress-bar__progress");

            healthBarWidget.style.position = Position.Absolute;
            healthBarWidget.style.left = position.x;
            healthBarWidget.style.top = position.y;

            _healthBars.Add(
                healthBarId,
                new HealthBarData
                {
                    HealthBarWidget = healthBarWidget,
                    HealthProgressBar = healthProgressBar,
                    HealthProgressBarProgress = progressBarProgress
                }
            );

            return healthBarId;
        }

        public void UpdateHealthBar(int healthBarId, Vector2 position, int currentHealth, int maxHealth)
        {
            if (_healthBars.TryGetValue(healthBarId, out var healthBarData))
            {
                var healthProgressBar = healthBarData.HealthProgressBar;
                healthProgressBar.lowValue = 0;
                healthProgressBar.highValue = maxHealth;
                healthProgressBar.value = currentHealth;

                var healthRatio = (float)currentHealth / maxHealth;
                var progressBarBackground = healthBarData.HealthProgressBarProgress;
                var healthColor = healthRatio <= 0.5
                    ? Color.Lerp(_lowHealthColor, _midHealthColor, healthRatio * 2)
                    : Color.Lerp(_midHealthColor, _fullHealthColor, (healthRatio - 0.5f) * 2);
                progressBarBackground.style.backgroundColor = healthColor;

                var healthBarWidget = healthBarData.HealthBarWidget;
                const int screenLeft = 0;
                var screenRight = Screen.width;
                const int screenTop = 0;
                var screenBottom = Screen.height;

                if (position.x < screenLeft || position.x > screenRight)
                {
                    healthBarWidget.style.display = DisplayStyle.None;
                }
                else if (position.y < screenTop || position.y > screenBottom)
                {
                    healthBarWidget.style.display = DisplayStyle.None;
                }
                else
                {
                    healthBarWidget.style.display = DisplayStyle.Flex;
                }

                healthBarWidget.style.left = position.x;
                healthBarWidget.style.top = position.y;
            }
        }

        public void DestroyHealthBar(int healthBarId)
        {
            if (_healthBars.TryGetValue(healthBarId, out var healthBarData))
            {
                _parent.Remove(healthBarData.HealthBarWidget);
                _healthBars.Remove(healthBarId);
            }
        }

        #endregion External Functions

        #region General

        private void Initialize()
        {
            var root = GameObject.FindWithTag(TagManager.UIRoot).GetComponent<UIDocument>().rootVisualElement;
            _parent = root.Q<VisualElement>(HealthBarParentString);
            _healthBars = new Dictionary<int, HealthBarData>();
            _healthBarCounterId = 0;
        }

        #endregion General

        #endregion Utils

        #region Structs

        private struct HealthBarData
        {
            public VisualElement HealthBarWidget;
            public ProgressBar HealthProgressBar;
            public VisualElement HealthProgressBarProgress;
        }

        #endregion Structs

        #region Singleton

        public static HUD_WorldHealthBarDisplayBar Instance { get; private set; }

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