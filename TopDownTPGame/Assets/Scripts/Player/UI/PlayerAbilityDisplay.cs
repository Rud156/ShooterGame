#region

using System;
using System.Collections.Generic;
using Player.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Utils.Input;
using Utils.Misc;

#endregion

namespace Player.UI
{
    public class PlayerAbilityDisplay : MonoBehaviour
    {
        private const string PrimaryDisplayString = "PrimaryAbility";
        private const string SecondaryDisplayString = "SecondaryAbility";
        private const string TertiaryDisplayString = "TertiaryAbility";
        private const string UltimateDisplayString = "UltimateAbility";

        [Header("Display")]
        [SerializeField] private Color _overlayColor;
        [SerializeField] [Range(0, 1)] private float _overlayMaxAlpha;

        private VisualElement _root;
        private AbilityDisplayItem _primaryDisplay;
        private AbilityDisplayItem _secondaryDisplay;
        private AbilityDisplayItem _tertiaryDisplay;
        private AbilityDisplayItem _ultimateDisplay;

        #region Utils

        private void Initialize()
        {
            _root = GameObject.FindWithTag(TagManager.UIRoot).GetComponent<UIDocument>().rootVisualElement;

            var primaryAbility = _root.Q<VisualElement>(PrimaryDisplayString);
            _primaryDisplay = new AbilityDisplayItem()
            {
                ItemRoot = primaryAbility,
                AbilityBackground = primaryAbility.Q<VisualElement>("Backing"),
                AbilityIcon = primaryAbility.Q<VisualElement>("AbilityIcon"),
                AbilityIconOverlay = primaryAbility.Q<VisualElement>("AbilityIconOverlay"),
                CooldownLabel = primaryAbility.Q<Label>("CooldownTimer"),
                StackCountLabel = primaryAbility.Q<Label>("StackCount"),
                AbilityTriggerLabel = primaryAbility.Q<Label>("AbilityTrigger")
            };

            var secondaryAbility = _root.Q<VisualElement>(SecondaryDisplayString);
            _secondaryDisplay = new AbilityDisplayItem()
            {
                ItemRoot = secondaryAbility,
                AbilityBackground = secondaryAbility.Q<VisualElement>("Backing"),
                AbilityIcon = secondaryAbility.Q<VisualElement>("AbilityIcon"),
                AbilityIconOverlay = secondaryAbility.Q<VisualElement>("AbilityIconOverlay"),
                CooldownLabel = secondaryAbility.Q<Label>("CooldownTimer"),
                StackCountLabel = secondaryAbility.Q<Label>("StackCount"),
                AbilityTriggerLabel = secondaryAbility.Q<Label>("AbilityTrigger")
            };

            var tertiaryAbility = _root.Q<VisualElement>(TertiaryDisplayString);
            _tertiaryDisplay = new AbilityDisplayItem()
            {
                ItemRoot = tertiaryAbility,
                AbilityBackground = tertiaryAbility.Q<VisualElement>("Backing"),
                AbilityIcon = tertiaryAbility.Q<VisualElement>("AbilityIcon"),
                AbilityIconOverlay = tertiaryAbility.Q<VisualElement>("AbilityIconOverlay"),
                CooldownLabel = tertiaryAbility.Q<Label>("CooldownTimer"),
                StackCountLabel = tertiaryAbility.Q<Label>("StackCount"),
                AbilityTriggerLabel = tertiaryAbility.Q<Label>("AbilityTrigger")
            };

            var ultimateAbility = _root.Q<VisualElement>(UltimateDisplayString);
            _ultimateDisplay = new AbilityDisplayItem()
            {
                ItemRoot = ultimateAbility,
                AbilityBackground = ultimateAbility.Q<VisualElement>("Backing"),
                AbilityIcon = ultimateAbility.Q<VisualElement>("AbilityIcon"),
                AbilityIconOverlay = ultimateAbility.Q<VisualElement>("AbilityIconOverlay"),
                CooldownLabel = ultimateAbility.Q<Label>("CooldownTimer"),
                StackCountLabel = ultimateAbility.Q<Label>("StackCount"),
                AbilityTriggerLabel = ultimateAbility.Q<Label>("AbilityTrigger")
            };
        }

        private void CheckAndApplyDisplayStyle(List<VisualElement> elements, bool show)
        {
            foreach (var element in elements)
            {
                element.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void DisplayCooldownLabelPercentAndOverlay(AbilityDisplayItem abilityDisplayItem, float percent, float maxPercent)
        {
            abilityDisplayItem.CooldownLabel.text = $"{percent:0.0} %";
            abilityDisplayItem.AbilityIconOverlay.style.unityBackgroundImageTintColor = percent >= maxPercent
                ? new Color(1, 1, 1, 0)
                : new Color(_overlayColor.r, _overlayColor.g, _overlayColor.b, _overlayMaxAlpha);
        }

        private void DisplayCooldownLabelTimerAndOverlay(AbilityDisplayItem abilityDisplayItem, float timer, float percent)
        {
            abilityDisplayItem.CooldownLabel.text = timer.ToString("0.0");
            abilityDisplayItem.AbilityIconOverlay.style.unityBackgroundImageTintColor = percent <= 0
                ? new Color(_overlayColor.r, _overlayColor.g, _overlayColor.b, 0)
                : new Color(_overlayColor.r, _overlayColor.g, _overlayColor.b, _overlayMaxAlpha);
        }

        #endregion Utils

        #region External Functions

        public void UpdateCooldownPercent(AbilityTrigger abilityTrigger, float percent, float maxPercent, bool show = true)
        {
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                {
                    var elements = new List<VisualElement>()
                    {
                        _primaryDisplay.CooldownLabel,
                        _primaryDisplay.AbilityIconOverlay,
                    };
                    CheckAndApplyDisplayStyle(elements, show);
                    DisplayCooldownLabelPercentAndOverlay(_primaryDisplay, percent, maxPercent);
                }
                    break;

                case AbilityTrigger.Secondary:
                {
                    var elements = new List<VisualElement>()
                    {
                        _secondaryDisplay.CooldownLabel,
                        _secondaryDisplay.AbilityIconOverlay,
                    };
                    CheckAndApplyDisplayStyle(elements, show);
                    DisplayCooldownLabelPercentAndOverlay(_secondaryDisplay, percent, maxPercent);
                }
                    break;

                case AbilityTrigger.Tertiary:
                {
                    var elements = new List<VisualElement>()
                    {
                        _tertiaryDisplay.CooldownLabel,
                        _tertiaryDisplay.AbilityIconOverlay,
                    };
                    CheckAndApplyDisplayStyle(elements, show);
                    DisplayCooldownLabelPercentAndOverlay(_tertiaryDisplay, percent, maxPercent);
                }
                    break;

                case AbilityTrigger.Ultimate:
                {
                    var elements = new List<VisualElement>()
                    {
                        _ultimateDisplay.CooldownLabel,
                        _ultimateDisplay.AbilityIconOverlay,
                    };
                    CheckAndApplyDisplayStyle(elements, show);
                    DisplayCooldownLabelPercentAndOverlay(_ultimateDisplay, percent, maxPercent);
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        public void UpdateCooldownTimer(AbilityTrigger abilityTrigger, float timer, float percent)
        {
            var show = percent > 0;

            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                {
                    var elements = new List<VisualElement>()
                    {
                        _primaryDisplay.CooldownLabel,
                        _primaryDisplay.AbilityIconOverlay,
                    };
                    CheckAndApplyDisplayStyle(elements, show);
                    DisplayCooldownLabelTimerAndOverlay(_primaryDisplay, timer, percent);
                }
                    break;

                case AbilityTrigger.Secondary:
                {
                    var elements = new List<VisualElement>()
                    {
                        _secondaryDisplay.CooldownLabel,
                        _secondaryDisplay.AbilityIconOverlay,
                    };
                    CheckAndApplyDisplayStyle(elements, show);
                    DisplayCooldownLabelTimerAndOverlay(_secondaryDisplay, timer, percent);
                }
                    break;

                case AbilityTrigger.Tertiary:
                {
                    var elements = new List<VisualElement>()
                    {
                        _tertiaryDisplay.CooldownLabel,
                        _tertiaryDisplay.AbilityIconOverlay,
                    };
                    CheckAndApplyDisplayStyle(elements, show);
                    DisplayCooldownLabelTimerAndOverlay(_tertiaryDisplay, timer, percent);
                }
                    break;

                case AbilityTrigger.Ultimate:
                {
                    var elements = new List<VisualElement>()
                    {
                        _ultimateDisplay.CooldownLabel,
                        _ultimateDisplay.AbilityIconOverlay,
                    };
                    CheckAndApplyDisplayStyle(elements, show);
                    DisplayCooldownLabelTimerAndOverlay(_ultimateDisplay, timer, percent);
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        public void UpdateAbilityTrigger(AbilityTrigger abilityTrigger)
        {
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                {
                    var displayString = InputManager.Instance.PlayerInput.AbilityPrimary.GetBindingDisplayString();
                    _primaryDisplay.AbilityTriggerLabel.text = displayString;
                }
                    break;

                case AbilityTrigger.Secondary:
                {
                    var displayString = InputManager.Instance.PlayerInput.AbilitySecondary.GetBindingDisplayString();
                    _secondaryDisplay.AbilityTriggerLabel.text = displayString;
                }
                    break;

                case AbilityTrigger.Tertiary:
                {
                    var displayString = InputManager.Instance.PlayerInput.AbilityTertiary.GetBindingDisplayString();
                    _tertiaryDisplay.AbilityTriggerLabel.text = displayString;
                }
                    break;

                case AbilityTrigger.Ultimate:
                {
                    var displayString = InputManager.Instance.PlayerInput.AbilityUltimate.GetBindingDisplayString();
                    _ultimateDisplay.AbilityTriggerLabel.text = displayString;
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        public void UpdateStackCount(AbilityTrigger abilityTrigger, int stackCount, bool show = true)
        {
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                {
                    _primaryDisplay.StackCountLabel.text = stackCount.ToString();
                    _primaryDisplay.StackCountLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                }
                    break;

                case AbilityTrigger.Secondary:
                {
                    _secondaryDisplay.StackCountLabel.text = stackCount.ToString();
                    _secondaryDisplay.StackCountLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                }
                    break;

                case AbilityTrigger.Tertiary:
                {
                    _tertiaryDisplay.StackCountLabel.text = stackCount.ToString();
                    _tertiaryDisplay.StackCountLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                }
                    break;

                case AbilityTrigger.Ultimate:
                {
                    _ultimateDisplay.StackCountLabel.text = stackCount.ToString();
                    _ultimateDisplay.StackCountLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                }
                    break;

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

                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        public void UpdateAbilityIcon(AbilityTrigger abilityTrigger, Sprite abilityIcon)
        {
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                {
                    _primaryDisplay.AbilityIcon.style.backgroundImage = new StyleBackground(abilityIcon);
                    _primaryDisplay.AbilityIconOverlay.style.backgroundImage = new StyleBackground(abilityIcon);
                }
                    break;

                case AbilityTrigger.Secondary:
                {
                    _secondaryDisplay.AbilityIcon.style.backgroundImage = new StyleBackground(abilityIcon);
                    _secondaryDisplay.AbilityIconOverlay.style.backgroundImage = new StyleBackground(abilityIcon);
                }
                    break;

                case AbilityTrigger.Tertiary:
                {
                    _tertiaryDisplay.AbilityIcon.style.backgroundImage = new StyleBackground(abilityIcon);
                    _tertiaryDisplay.AbilityIconOverlay.style.backgroundImage = new StyleBackground(abilityIcon);
                }
                    break;

                case AbilityTrigger.Ultimate:
                {
                    _ultimateDisplay.AbilityIcon.style.backgroundImage = new StyleBackground(abilityIcon);
                    _ultimateDisplay.AbilityIconOverlay.style.backgroundImage = new StyleBackground(abilityIcon);
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        #endregion External Functions

        #region Singleton

        public static PlayerAbilityDisplay Instance { get; private set; }

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
            public Label CooldownLabel;
            public Label StackCountLabel;
            public VisualElement AbilityBackground;
            public VisualElement AbilityIcon;
            public VisualElement AbilityIconOverlay;
            public Label AbilityTriggerLabel;
        }

        #endregion Structs
    }
}