#region

using System;
using Player.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Utils.Input;
using Utils.Misc;

#endregion

namespace UI.Player
{
    public class HUD_PlayerAbilityDisplay : MonoBehaviour
    {
        private const string PrimaryDisplayString = "PrimaryAbility";
        private const string SecondaryDisplayString = "SecondaryAbility";
        private const string TertiaryDisplayString = "TertiaryAbility";
        private const string UltimateDisplayString = "UltimateAbility";

        private const float AbilityIconOverlayHeight = 90;

        private AbilityDisplayItem _primaryDisplay;
        private AbilityDisplayItem _secondaryDisplay;
        private AbilityDisplayItem _tertiaryDisplay;
        private AbilityDisplayItem _ultimateDisplay;

        #region Unity Functions

        private void Start()
        {
            var deviceGroupName = CustomInputManager.Instance.LastUsedDeviceInputType;
            UpdateAllAbilityTriggers(deviceGroupName);
        }

        private void OnDestroy() => CustomInputManager.Instance.OnLastUsedInputChanged -= UpdateAllAbilityTriggers;

        #endregion Unity Functions

        #region Utils

        private void Initialize()
        {
            var root = GameObject.FindWithTag(TagManager.UIRoot).GetComponent<UIDocument>().rootVisualElement;

            var primaryAbility = root.Q<VisualElement>(PrimaryDisplayString);
            _primaryDisplay = new AbilityDisplayItem()
            {
                AbilityBackground = primaryAbility.Q<VisualElement>("BackgroundImage"),
                AbilityIcon = primaryAbility.Q<VisualElement>("AbilityIcon"),
                AbilityIconOverlayBacker = primaryAbility.Q<VisualElement>("AbilityIconOverlayBacker"),
                AbilityIconOverlay = primaryAbility.Q<VisualElement>("AbilityIconOverlay"),
                AbilityTriggerLabel = primaryAbility.Q<Label>("TriggerName"),
                TimerLabel = primaryAbility.Q<Label>("Timer"),
                CounterLabel = primaryAbility.Q<Label>("Counter"),
            };

            var secondaryAbility = root.Q<VisualElement>(SecondaryDisplayString);
            _secondaryDisplay = new AbilityDisplayItem()
            {
                AbilityBackground = secondaryAbility.Q<VisualElement>("BackgroundImage"),
                AbilityIcon = secondaryAbility.Q<VisualElement>("AbilityIcon"),
                AbilityIconOverlayBacker = secondaryAbility.Q<VisualElement>("AbilityIconOverlayBacker"),
                AbilityIconOverlay = secondaryAbility.Q<VisualElement>("AbilityIconOverlay"),
                AbilityTriggerLabel = secondaryAbility.Q<Label>("TriggerName"),
                TimerLabel = secondaryAbility.Q<Label>("Timer"),
                CounterLabel = secondaryAbility.Q<Label>("Counter"),
            };

            var tertiaryAbility = root.Q<VisualElement>(TertiaryDisplayString);
            _tertiaryDisplay = new AbilityDisplayItem()
            {
                AbilityBackground = tertiaryAbility.Q<VisualElement>("BackgroundImage"),
                AbilityIcon = tertiaryAbility.Q<VisualElement>("AbilityIcon"),
                AbilityIconOverlayBacker = tertiaryAbility.Q<VisualElement>("AbilityIconOverlayBacker"),
                AbilityIconOverlay = tertiaryAbility.Q<VisualElement>("AbilityIconOverlay"),
                AbilityTriggerLabel = tertiaryAbility.Q<Label>("TriggerName"),
                TimerLabel = tertiaryAbility.Q<Label>("Timer"),
                CounterLabel = tertiaryAbility.Q<Label>("Counter"),
            };

            var ultimateAbility = root.Q<VisualElement>(UltimateDisplayString);
            _ultimateDisplay = new AbilityDisplayItem()
            {
                AbilityBackground = ultimateAbility.Q<VisualElement>("BackgroundImage"),
                AbilityIcon = ultimateAbility.Q<VisualElement>("AbilityIcon"),
                AbilityIconOverlayBacker = ultimateAbility.Q<VisualElement>("AbilityIconOverlayBacker"),
                AbilityIconOverlay = ultimateAbility.Q<VisualElement>("AbilityIconOverlay"),
                AbilityTriggerLabel = ultimateAbility.Q<Label>("TriggerName"),
                TimerLabel = ultimateAbility.Q<Label>("Timer"),
                CounterLabel = ultimateAbility.Q<Label>("Counter"),
            };

            CustomInputManager.Instance.OnLastUsedInputChanged += UpdateAllAbilityTriggers;
        }

        private void UpdateAbilityTrigger(AbilityTrigger abilityTrigger, string lastInputType)
        {
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                {
                    var displayString = CustomInputManager.Instance.PlayerInput.AbilityPrimary.GetBindingDisplayString(group: lastInputType);
                    _primaryDisplay.AbilityTriggerLabel.text = displayString;
                }
                    break;

                case AbilityTrigger.Secondary:
                {
                    var displayString = CustomInputManager.Instance.PlayerInput.AbilitySecondary.GetBindingDisplayString(group: lastInputType);
                    _secondaryDisplay.AbilityTriggerLabel.text = displayString;
                }
                    break;

                case AbilityTrigger.Tertiary:
                {
                    var displayString = CustomInputManager.Instance.PlayerInput.AbilityTertiary.GetBindingDisplayString(group: lastInputType);
                    _tertiaryDisplay.AbilityTriggerLabel.text = displayString;
                }
                    break;

                case AbilityTrigger.Ultimate:
                {
                    var displayString = CustomInputManager.Instance.PlayerInput.AbilityUltimate.GetBindingDisplayString(group: lastInputType);
                    _ultimateDisplay.AbilityTriggerLabel.text = displayString;
                }
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        #endregion Utils

        #region External Functions

        private void UpdateAllAbilityTriggers(string currentInputGroup)
        {
            UpdateAbilityTrigger(AbilityTrigger.Primary, currentInputGroup);
            UpdateAbilityTrigger(AbilityTrigger.Secondary, currentInputGroup);
            UpdateAbilityTrigger(AbilityTrigger.Tertiary, currentInputGroup);
            UpdateAbilityTrigger(AbilityTrigger.Ultimate, currentInputGroup);
        }

        public void UpdateTimer(AbilityTrigger abilityTrigger, string timerString, bool showTimer)
        {
            var displayStyle = showTimer ? DisplayStyle.Flex : DisplayStyle.None;

            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                    _primaryDisplay.TimerLabel.text = timerString;
                    _primaryDisplay.TimerLabel.style.display = displayStyle;
                    break;

                case AbilityTrigger.Secondary:
                    _secondaryDisplay.TimerLabel.text = timerString;
                    _secondaryDisplay.TimerLabel.style.display = displayStyle;
                    break;

                case AbilityTrigger.Tertiary:
                    _tertiaryDisplay.TimerLabel.text = timerString;
                    _tertiaryDisplay.TimerLabel.style.display = displayStyle;
                    break;

                case AbilityTrigger.Ultimate:
                    _ultimateDisplay.TimerLabel.text = timerString;
                    _ultimateDisplay.TimerLabel.style.display = displayStyle;
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
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
                    _primaryDisplay.AbilityIconOverlay.style.display = displayStyle;
                    _primaryDisplay.AbilityIconOverlayBacker.style.display = displayStyle;
                    _primaryDisplay.AbilityIconOverlay.style.height = mappedHeight;
                    break;

                case AbilityTrigger.Secondary:
                    _secondaryDisplay.AbilityIconOverlay.style.display = displayStyle;
                    _secondaryDisplay.AbilityIconOverlayBacker.style.display = displayStyle;
                    _secondaryDisplay.AbilityIconOverlay.style.height = mappedHeight;
                    break;

                case AbilityTrigger.Tertiary:
                    _tertiaryDisplay.AbilityIconOverlay.style.display = displayStyle;
                    _tertiaryDisplay.AbilityIconOverlayBacker.style.display = displayStyle;
                    _tertiaryDisplay.AbilityIconOverlay.style.height = mappedHeight;
                    break;

                case AbilityTrigger.Ultimate:
                    _ultimateDisplay.AbilityIconOverlay.style.display = displayStyle;
                    _ultimateDisplay.AbilityIconOverlayBacker.style.display = displayStyle;
                    _ultimateDisplay.AbilityIconOverlay.style.height = mappedHeight;
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
                    _primaryDisplay.CounterLabel.text = counterData;
                    _primaryDisplay.CounterLabel.style.display = displayStyle;
                }
                    break;

                case AbilityTrigger.Secondary:
                {
                    _secondaryDisplay.CounterLabel.text = counterData;
                    _secondaryDisplay.CounterLabel.style.display = displayStyle;
                }
                    break;

                case AbilityTrigger.Tertiary:
                {
                    _tertiaryDisplay.CounterLabel.text = counterData;
                    _tertiaryDisplay.CounterLabel.style.display = displayStyle;
                }
                    break;

                case AbilityTrigger.Ultimate:
                {
                    _ultimateDisplay.CounterLabel.text = counterData;
                    _ultimateDisplay.CounterLabel.style.display = displayStyle;
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
                    _primaryDisplay.AbilityBackground.style.backgroundImage = new StyleBackground(abilityBackground);
                    _primaryDisplay.AbilityBackground.style.unityBackgroundImageTintColor = backgroundTint;
                    break;

                case AbilityTrigger.Secondary:
                    _secondaryDisplay.AbilityBackground.style.backgroundImage = new StyleBackground(abilityBackground);
                    _secondaryDisplay.AbilityBackground.style.unityBackgroundImageTintColor = backgroundTint;
                    break;

                case AbilityTrigger.Tertiary:
                    _tertiaryDisplay.AbilityBackground.style.backgroundImage = new StyleBackground(abilityBackground);
                    _tertiaryDisplay.AbilityBackground.style.unityBackgroundImageTintColor = backgroundTint;
                    break;

                case AbilityTrigger.Ultimate:
                    _ultimateDisplay.AbilityBackground.style.backgroundImage = new StyleBackground(abilityBackground);
                    _ultimateDisplay.AbilityBackground.style.unityBackgroundImageTintColor = backgroundTint;
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
                    _primaryDisplay.AbilityIcon.style.backgroundImage = new StyleBackground(abilityIcon);
                    break;

                case AbilityTrigger.Secondary:
                    _secondaryDisplay.AbilityIcon.style.backgroundImage = new StyleBackground(abilityIcon);
                    break;

                case AbilityTrigger.Tertiary:
                    _tertiaryDisplay.AbilityIcon.style.backgroundImage = new StyleBackground(abilityIcon);
                    break;

                case AbilityTrigger.Ultimate:
                    _ultimateDisplay.AbilityIcon.style.backgroundImage = new StyleBackground(abilityIcon);
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        #endregion External Functions

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
            public Label TimerLabel;
            public Label CounterLabel;
            public VisualElement AbilityBackground;
            public VisualElement AbilityIcon;
            public VisualElement AbilityIconOverlay;
            public VisualElement AbilityIconOverlayBacker;
            public Label AbilityTriggerLabel;
        }

        #endregion Structs
    }
}