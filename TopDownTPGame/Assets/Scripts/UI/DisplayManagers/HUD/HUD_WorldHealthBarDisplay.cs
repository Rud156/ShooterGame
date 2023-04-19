#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Misc;

#endregion

namespace UI.DisplayManagers.HUD
{
    public class HUD_WorldHealthBarDisplay : MonoBehaviour
    {
        private const string HealthBarParentString = "UI_WorldHealthBarContainer";
        private const string HealthBarLabelString = "HealthProgressText";

        [Header("Prefabs")]
        [SerializeField] private VisualTreeAsset _healthBarPrefab;

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
            var healthBarLabel = healthBarWidget.Q<Label>(HealthBarLabelString);
            _parent.Add(healthBarWidget);

            healthBarLabel.text = $"{currentHealth}/{maxHealth}";
            healthBarWidget.style.position = Position.Absolute;
            healthBarWidget.style.left = position.x;
            healthBarWidget.style.top = position.y;

            _healthBars.Add(
                healthBarId,
                new HealthBarData
                {
                    healthBarWidget = healthBarWidget,
                    healthBarLabel = healthBarLabel
                }
            );

            return healthBarId;
        }

        public void UpdateHealthBar(int healthBarId, Vector2 position, int currentHealth, int maxHealth)
        {
            if (_healthBars.TryGetValue(healthBarId, out var healthBarData))
            {
                var textLabel = healthBarData.healthBarLabel;
                textLabel.text = $"{currentHealth}/{maxHealth}";

                var healthBarWidget = healthBarData.healthBarWidget;
                healthBarWidget.style.left = position.x;
                healthBarWidget.style.top = position.y;
            }
        }

        public void DestroyHealthBar(int healthBarId)
        {
            if (_healthBars.TryGetValue(healthBarId, out var healthBarData))
            {
                _parent.Remove(healthBarData.healthBarWidget);
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
            public VisualElement healthBarWidget;
            public Label healthBarLabel;
        }

        #endregion Structs

        #region Singleton

        public static HUD_WorldHealthBarDisplay Instance { get; private set; }

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