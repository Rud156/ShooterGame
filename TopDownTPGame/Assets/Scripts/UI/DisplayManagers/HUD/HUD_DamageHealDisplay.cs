#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Misc;

#endregion

namespace UI.HUD
{
    public class HUD_DamageHealDisplay : MonoBehaviour
    {
        private const string DamageOrHealParentString = "UI_DamageOrHealNumberContainer";
        private const string DamageOrHealLabelString = "HealthOrDamageLabel";

        [Header("Prefabs")]
        [SerializeField] private VisualTreeAsset _damageOrHealPrefab;

        [Header("Colors")]
        [SerializeField] private Color _damageColor;
        [SerializeField] private Color _healColor;

        [Header("Display Data")]
        [SerializeField] private float _fadeInDuration;
        [SerializeField] private float _stayDuration;
        [SerializeField] private float _fadeOutDuration;

        private VisualElement _parent;
        private List<DamageOrHealData> _damageOrHealWidgets;
        private int _damageOrHealWidgetId;

        #region Unity Functions

        private void Update() => UpdateDamageOrHealWidgets();

        #endregion Unity Functions

        #region External Functions

        public int AddHealWidget(Vector2 position, int healAmount)
        {
            var healWidget = _damageOrHealPrefab.Instantiate();
            _damageOrHealWidgetId += 1;
            _parent.Add(healWidget);

            var healLabel = healWidget.Q<Label>(DamageOrHealLabelString);
            healLabel.style.color = _healColor;
            healLabel.text = $"+{healAmount}";

            healWidget.style.position = Position.Absolute;
            healWidget.style.left = position.x;
            healWidget.style.top = position.y;

            _damageOrHealWidgets.Add(new DamageOrHealData
            {
                DamageOrHealWidget = healWidget,
                DamageOrHealLabel = healLabel,
                CurrentTime = _fadeInDuration,
                DamageOrHealWidgetState = DamageOrHealState.FadeIn,
                WidgetId = _damageOrHealWidgetId,
            });

            return _damageOrHealWidgetId;
        }

        public int AddDamageWidget(Vector2 position, int damageAmount)
        {
            var damageWidget = _damageOrHealPrefab.Instantiate();
            _damageOrHealWidgetId += 1;
            _parent.Add(damageWidget);

            var damageLabel = damageWidget.Q<Label>(DamageOrHealLabelString);
            damageLabel.style.color = _damageColor;
            damageLabel.text = $"-{damageAmount}";

            damageWidget.style.position = Position.Absolute;
            damageWidget.style.left = position.x;
            damageWidget.style.top = position.y;

            _damageOrHealWidgets.Add(new DamageOrHealData
            {
                DamageOrHealWidget = damageWidget,
                DamageOrHealLabel = damageLabel,
                CurrentTime = _fadeInDuration,
                DamageOrHealWidgetState = DamageOrHealState.FadeIn,
                WidgetId = _damageOrHealWidgetId,
            });

            return _damageOrHealWidgetId;
        }

        public bool UpdateWidgetPosition(int widgetId, Vector2 position)
        {
            var index = -1;
            for (var i = 0; i < _damageOrHealWidgets.Count; i++)
            {
                if (_damageOrHealWidgets[i].WidgetId == widgetId)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                _damageOrHealWidgets[index].DamageOrHealWidget.style.left = position.x;
                _damageOrHealWidgets[index].DamageOrHealWidget.style.top = position.y;
                return true;
            }

            return false;
        }

        #endregion External Functions

        #region Utils

        private void UpdateDamageOrHealWidgets()
        {
            for (var i = _damageOrHealWidgets.Count - 1; i >= 0; i--)
            {
                var widgetData = _damageOrHealWidgets[i];
                switch (widgetData.DamageOrHealWidgetState)
                {
                    case DamageOrHealState.FadeIn:
                    {
                        var lerpSpeed = 1 / _fadeInDuration;
                        widgetData.LerpAmount += Time.deltaTime * lerpSpeed;

                        var mappedAlpha = Mathf.Lerp(0, 1, widgetData.LerpAmount);
                        widgetData.DamageOrHealLabel.style.opacity = mappedAlpha;

                        widgetData.CurrentTime -= Time.deltaTime;
                        if (widgetData.CurrentTime <= 0)
                        {
                            widgetData.DamageOrHealWidgetState = DamageOrHealState.Stay;
                            widgetData.CurrentTime = _stayDuration;
                        }

                        _damageOrHealWidgets[i] = widgetData;
                    }
                        break;

                    case DamageOrHealState.Stay:
                    {
                        widgetData.CurrentTime -= Time.deltaTime;
                        if (widgetData.CurrentTime <= 0)
                        {
                            widgetData.DamageOrHealWidgetState = DamageOrHealState.FadeOut;
                            widgetData.CurrentTime = _fadeOutDuration;
                        }

                        _damageOrHealWidgets[i] = widgetData;
                    }
                        break;

                    case DamageOrHealState.FadeOut:
                    {
                        var lerpSpeed = 1 / _fadeOutDuration;
                        widgetData.LerpAmount += Time.deltaTime * lerpSpeed;

                        var mappedAlpha = Mathf.Lerp(1, 0, widgetData.LerpAmount);
                        widgetData.DamageOrHealLabel.style.opacity = mappedAlpha;

                        widgetData.CurrentTime -= Time.deltaTime;
                        if (widgetData.CurrentTime <= 0)
                        {
                            widgetData.DamageOrHealWidgetState = DamageOrHealState.Destroy;
                        }

                        _damageOrHealWidgets[i] = widgetData;
                    }
                        break;

                    case DamageOrHealState.Destroy:
                    {
                        widgetData.CurrentTime -= Time.deltaTime;
                        if (widgetData.CurrentTime <= 0)
                        {
                            _parent.Remove(widgetData.DamageOrHealWidget);
                            _damageOrHealWidgets.RemoveAt(i);
                        }
                    }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void Initialize()
        {
            var root = GameObject.FindWithTag(TagManager.UIRoot).GetComponent<UIDocument>().rootVisualElement;
            _parent = root.Q<VisualElement>(DamageOrHealParentString);
            _damageOrHealWidgets = new List<DamageOrHealData>();
            _damageOrHealWidgetId = 0;
        }

        #endregion Utils

        #region Structs

        private struct DamageOrHealData
        {
            public VisualElement DamageOrHealWidget;
            public Label DamageOrHealLabel;
            public DamageOrHealState DamageOrHealWidgetState;
            public float CurrentTime;
            public float LerpAmount;
            public int WidgetId;
        }

        #endregion Structs

        #region Enums

        private enum DamageOrHealState
        {
            FadeIn,
            Stay,
            FadeOut,
            Destroy
        }

        #endregion Enums

        #region Singleton

        public static HUD_DamageHealDisplay Instance { get; private set; }

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