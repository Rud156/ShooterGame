using Player.Abilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Utils.Input;
using Utils.Misc;

namespace UI.Player
{
    public class HUD_PlayerAbilityDisplay : MonoBehaviour
    {
        private const string PrimaryDisplayString = "PrimaryAbility";
        private const string SecondaryDisplayString = "SecondaryAbility";

        private const float AbilityIconOverlayHeight = 90;

        [Header("Ability Flasher")]
        [SerializeField] private int _flashCount;
        [SerializeField] private float _flashOnDuration;
        [SerializeField] private float _flashOffDuration;

        [Header("Ability Scaler")]
        [SerializeField] private int _scaleCount;
        [SerializeField] private float _defaultScale;
        [SerializeField] private float _biggerScale;
        [SerializeField] private float _scaleChangeDuration;

        private AbilityDisplayItem _primaryAbilityDisplay;
        private AbilityDisplayItem _secondaryAbilityDisplay;

        private CoroutineData _flashCoroutineData;
        private CoroutineData _scaleCoroutineData;

        #region Unity Functions

        private void Start()
        {
            _flashCoroutineData = new CoroutineData()
            {
                PrimaryActive = false,
                SecondaryActive = false
            };
            _scaleCoroutineData = new CoroutineData()
            {
                PrimaryActive = false,
                SecondaryActive = false
            };

            var deviceGroupName = CustomInputManager.Instance.LastUsedDeviceInputType;
            UpdateAllAbilityTriggers(deviceGroupName);
        }

        private void OnDestroy() => CustomInputManager.Instance.OnLastUsedInputChanged -= UpdateAllAbilityTriggers;

        #endregion Unity Functions

        #region Ability Setup and Updates

        private void Initialize()
        {
            var root = GameObject.FindWithTag(TagManager.UIRoot).GetComponent<UIDocument>().rootVisualElement;

            var primaryAbility = root.Q<VisualElement>(PrimaryDisplayString);
            _primaryAbilityDisplay = new AbilityDisplayItem()
            {
                ItemRoot = primaryAbility,
                AbilityBackground = primaryAbility.Q<VisualElement>("BackgroundImage"),
                AbilityIcon = primaryAbility.Q<VisualElement>("AbilityIcon"),
                AbilityIconOverlayBacker = primaryAbility.Q<VisualElement>("AbilityIconOverlayBacker"),
                AbilityIconOverlay = primaryAbility.Q<VisualElement>("AbilityIconOverlay"),
                AbilityTriggerLabel = primaryAbility.Q<Label>("TriggerName"),
                TimerLabel = primaryAbility.Q<Label>("Timer"),
                CounterLabel = primaryAbility.Q<Label>("Counter"),
                Flasher = primaryAbility.Q<VisualElement>("Flasher"),
            };

            var secondaryAbility = root.Q<VisualElement>(SecondaryDisplayString);
            _secondaryAbilityDisplay = new AbilityDisplayItem()
            {
                ItemRoot = secondaryAbility,
                AbilityBackground = secondaryAbility.Q<VisualElement>("BackgroundImage"),
                AbilityIcon = secondaryAbility.Q<VisualElement>("AbilityIcon"),
                AbilityIconOverlayBacker = secondaryAbility.Q<VisualElement>("AbilityIconOverlayBacker"),
                AbilityIconOverlay = secondaryAbility.Q<VisualElement>("AbilityIconOverlay"),
                AbilityTriggerLabel = secondaryAbility.Q<Label>("TriggerName"),
                TimerLabel = secondaryAbility.Q<Label>("Timer"),
                CounterLabel = secondaryAbility.Q<Label>("Counter"),
                Flasher = secondaryAbility.Q<VisualElement>("Flasher"),
            };
        }

        private void UpdateAbilityTrigger(AbilityTrigger abilityTrigger, string lastInputType)
        {
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                    {
                        var displayString = CustomInputManager.Instance.PlayerInput.AbilityPrimary.GetBindingDisplayString(group: lastInputType);
                        _primaryAbilityDisplay.AbilityTriggerLabel.text = displayString;
                    }
                    break;

                case AbilityTrigger.Secondary:
                    {
                        var displayString = CustomInputManager.Instance.PlayerInput.AbilitySecondary.GetBindingDisplayString(group: lastInputType);
                        _secondaryAbilityDisplay.AbilityTriggerLabel.text = displayString;
                    }
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        private void UpdateAllAbilityTriggers(InputType currentInputGroup)
        {
            var inputGroupString = currentInputGroup == InputType.GamePad ? CustomInputManager.GamepadGroupString : CustomInputManager.KeyboardMouseGroupString;
            UpdateAbilityTrigger(AbilityTrigger.Primary, inputGroupString);
            UpdateAbilityTrigger(AbilityTrigger.Secondary, inputGroupString);
        }

        public void UpdateTimer(AbilityTrigger abilityTrigger, string timerString, bool showTimer)
        {
            Label displayLabel = null;
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                    displayLabel = _primaryAbilityDisplay.TimerLabel;
                    break;

                case AbilityTrigger.Secondary:
                    displayLabel = _secondaryAbilityDisplay.TimerLabel;
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }

            if (displayLabel != null)
            {
                displayLabel.text = timerString;
                displayLabel.style.display = showTimer ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public void UpdateOverlay(AbilityTrigger abilityTrigger, float percent)
        {
            var show = percent > 0;
            var displayStyle = show ? DisplayStyle.Flex : DisplayStyle.None;
            var mappedHeight = ExtensionFunctions.Map(percent, 0, 1, 0, AbilityIconOverlayHeight);

            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                    _primaryAbilityDisplay.AbilityIconOverlay.style.display = displayStyle;
                    _primaryAbilityDisplay.AbilityIconOverlayBacker.style.display = displayStyle;
                    _primaryAbilityDisplay.AbilityIconOverlay.style.height = mappedHeight;
                    break;

                case AbilityTrigger.Secondary:
                    _secondaryAbilityDisplay.AbilityIconOverlay.style.display = displayStyle;
                    _secondaryAbilityDisplay.AbilityIconOverlayBacker.style.display = displayStyle;
                    _secondaryAbilityDisplay.AbilityIconOverlay.style.height = mappedHeight;
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        public void UpdateCounter(AbilityTrigger abilityTrigger, string counterData, bool show)
        {
            var displayStyle = show ? DisplayStyle.Flex : DisplayStyle.None;
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                    {
                        _primaryAbilityDisplay.CounterLabel.text = counterData;
                        _primaryAbilityDisplay.CounterLabel.style.display = displayStyle;
                    }
                    break;

                case AbilityTrigger.Secondary:
                    {
                        _secondaryAbilityDisplay.CounterLabel.text = counterData;
                        _secondaryAbilityDisplay.CounterLabel.style.display = displayStyle;
                    }
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        public void UpdateAbilityBackground(AbilityTrigger abilityTrigger, Sprite abilityBackground, Color backgroundTint)
        {
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                    _primaryAbilityDisplay.AbilityBackground.style.backgroundImage = new StyleBackground(abilityBackground);
                    _primaryAbilityDisplay.AbilityBackground.style.unityBackgroundImageTintColor = backgroundTint;
                    break;

                case AbilityTrigger.Secondary:
                    _secondaryAbilityDisplay.AbilityBackground.style.backgroundImage = new StyleBackground(abilityBackground);
                    _secondaryAbilityDisplay.AbilityBackground.style.unityBackgroundImageTintColor = backgroundTint;
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        public void UpdateAbilityIcon(AbilityTrigger abilityTrigger, Sprite abilityIcon)
        {
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                    _primaryAbilityDisplay.AbilityIcon.style.backgroundImage = new StyleBackground(abilityIcon);
                    break;

                case AbilityTrigger.Secondary:
                    _secondaryAbilityDisplay.AbilityIcon.style.backgroundImage = new StyleBackground(abilityIcon);
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        #endregion Ability Setup and Updates

        #region Ability Flash and Scale

        private IEnumerator FlashCoroutine(VisualElement flasher, AbilityTrigger abilityTrigger)
        {
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                    _flashCoroutineData.PrimaryActive = true;
                    break;

                case AbilityTrigger.Secondary:
                    _flashCoroutineData.SecondaryActive = true;
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }

            for (var i = 0; i < _flashCount; i++)
            {
                flasher.style.display = DisplayStyle.Flex;
                yield return new WaitForSeconds(_flashOnDuration);
                flasher.style.display = DisplayStyle.None;
                yield return new WaitForSeconds(_flashOffDuration);
            }

            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                    _flashCoroutineData.PrimaryActive = false;
                    break;

                case AbilityTrigger.Secondary:
                    _flashCoroutineData.SecondaryActive = false;
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        private IEnumerator ScaleCoroutine(VisualElement scaler, AbilityTrigger abilityTrigger)
        {
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                    _scaleCoroutineData.PrimaryActive = true;
                    break;

                case AbilityTrigger.Secondary:
                    _scaleCoroutineData.SecondaryActive = true;
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }

            for (var i = 0; i < _scaleCount; i++)
            {
                scaler.style.scale = Vector2.one * _biggerScale;
                yield return new WaitForSeconds(_scaleChangeDuration);
                scaler.style.scale = Vector2.one * _defaultScale;
                yield return new WaitForSeconds(_scaleChangeDuration);
            }

            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                    _scaleCoroutineData.PrimaryActive = false;
                    break;

                case AbilityTrigger.Secondary:
                    _scaleCoroutineData.SecondaryActive = false;
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        public void TriggerAbilityFlashAndScale(AbilityTrigger abilityTrigger)
        {
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                    {
                        if (!_flashCoroutineData.PrimaryActive)
                        {
                            StartCoroutine(FlashCoroutine(_primaryAbilityDisplay.Flasher, AbilityTrigger.Primary));
                        }

                        if (!_scaleCoroutineData.PrimaryActive)
                        {
                            StartCoroutine(ScaleCoroutine(_primaryAbilityDisplay.ItemRoot, AbilityTrigger.Primary));
                        }
                    }
                    break;

                case AbilityTrigger.Secondary:
                    {
                        if (!_flashCoroutineData.SecondaryActive)
                        {
                            StartCoroutine(FlashCoroutine(_secondaryAbilityDisplay.Flasher, AbilityTrigger.Secondary));
                        }

                        if (!_scaleCoroutineData.PrimaryActive)
                        {
                            StartCoroutine(ScaleCoroutine(_secondaryAbilityDisplay.ItemRoot, AbilityTrigger.Secondary));
                        }
                    }
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        #endregion Ability Flash and Scale

        #region Singleton

        public static HUD_PlayerAbilityDisplay Instance { get; private set; }

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

        #region Structs

        private struct AbilityDisplayItem
        {
            public VisualElement ItemRoot;
            public Label TimerLabel;
            public Label CounterLabel;
            public VisualElement AbilityBackground;
            public VisualElement AbilityIcon;
            public VisualElement AbilityIconOverlay;
            public VisualElement AbilityIconOverlayBacker;
            public Label AbilityTriggerLabel;
            public VisualElement Flasher;
        }

        private struct CoroutineData
        {
            [FormerlySerializedAs("_primaryActive")]
            public bool PrimaryActive;
            [FormerlySerializedAs("secondaryActive")]
            [FormerlySerializedAs("_secondaryActive")]
            public bool SecondaryActive;
        }

        #endregion Structs
    }
}