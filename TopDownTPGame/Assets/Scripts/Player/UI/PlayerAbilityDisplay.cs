﻿#region

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
                itemRoot = primaryAbility,
                abilityBackground = primaryAbility.Q<VisualElement>("Backing"),
                abilityIcon = primaryAbility.Q<VisualElement>("AbilityIcon"),
                abilityIconOverlay = primaryAbility.Q<VisualElement>("AbilityIconOverlay"),
                cooldownLabel = primaryAbility.Q<Label>("CooldownTimer"),
                stackCountLabel = primaryAbility.Q<Label>("StackCount"),
                abilityTriggerLabel = primaryAbility.Q<Label>("AbilityTrigger")
            };

            var secondaryAbility = _root.Q<VisualElement>(SecondaryDisplayString);
            _secondaryDisplay = new AbilityDisplayItem()
            {
                itemRoot = secondaryAbility,
                abilityBackground = secondaryAbility.Q<VisualElement>("Backing"),
                abilityIcon = secondaryAbility.Q<VisualElement>("AbilityIcon"),
                abilityIconOverlay = secondaryAbility.Q<VisualElement>("AbilityIconOverlay"),
                cooldownLabel = secondaryAbility.Q<Label>("CooldownTimer"),
                stackCountLabel = secondaryAbility.Q<Label>("StackCount"),
                abilityTriggerLabel = secondaryAbility.Q<Label>("AbilityTrigger")
            };

            var tertiaryAbility = _root.Q<VisualElement>(TertiaryDisplayString);
            _tertiaryDisplay = new AbilityDisplayItem()
            {
                itemRoot = tertiaryAbility,
                abilityBackground = tertiaryAbility.Q<VisualElement>("Backing"),
                abilityIcon = tertiaryAbility.Q<VisualElement>("AbilityIcon"),
                abilityIconOverlay = tertiaryAbility.Q<VisualElement>("AbilityIconOverlay"),
                cooldownLabel = tertiaryAbility.Q<Label>("CooldownTimer"),
                stackCountLabel = tertiaryAbility.Q<Label>("StackCount"),
                abilityTriggerLabel = tertiaryAbility.Q<Label>("AbilityTrigger")
            };

            var ultimateAbility = _root.Q<VisualElement>(UltimateDisplayString);
            _ultimateDisplay = new AbilityDisplayItem()
            {
                itemRoot = ultimateAbility,
                abilityBackground = ultimateAbility.Q<VisualElement>("Backing"),
                abilityIcon = ultimateAbility.Q<VisualElement>("AbilityIcon"),
                abilityIconOverlay = ultimateAbility.Q<VisualElement>("AbilityIconOverlay"),
                cooldownLabel = ultimateAbility.Q<Label>("CooldownTimer"),
                stackCountLabel = ultimateAbility.Q<Label>("StackCount"),
                abilityTriggerLabel = ultimateAbility.Q<Label>("AbilityTrigger")
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
            abilityDisplayItem.cooldownLabel.text = $"{percent:0.0} %";
            abilityDisplayItem.abilityIconOverlay.style.unityBackgroundImageTintColor = percent >= maxPercent
                ? new Color(1, 1, 1, 0)
                : new Color(_overlayColor.r, _overlayColor.g, _overlayColor.b, _overlayMaxAlpha);
        }

        private void DisplayCooldownLabelTimerAndOverlay(AbilityDisplayItem abilityDisplayItem, float timer, float percent)
        {
            abilityDisplayItem.cooldownLabel.text = timer.ToString("0.0");
            abilityDisplayItem.abilityIconOverlay.style.unityBackgroundImageTintColor = percent <= 0
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
                        _primaryDisplay.cooldownLabel,
                        _primaryDisplay.abilityIconOverlay,
                    };
                    CheckAndApplyDisplayStyle(elements, show);
                    DisplayCooldownLabelPercentAndOverlay(_primaryDisplay, percent, maxPercent);
                }
                    break;

                case AbilityTrigger.Secondary:
                {
                    var elements = new List<VisualElement>()
                    {
                        _secondaryDisplay.cooldownLabel,
                        _secondaryDisplay.abilityIconOverlay,
                    };
                    CheckAndApplyDisplayStyle(elements, show);
                    DisplayCooldownLabelPercentAndOverlay(_secondaryDisplay, percent, maxPercent);
                }
                    break;

                case AbilityTrigger.Tertiary:
                {
                    var elements = new List<VisualElement>()
                    {
                        _tertiaryDisplay.cooldownLabel,
                        _tertiaryDisplay.abilityIconOverlay,
                    };
                    CheckAndApplyDisplayStyle(elements, show);
                    DisplayCooldownLabelPercentAndOverlay(_tertiaryDisplay, percent, maxPercent);
                }
                    break;

                case AbilityTrigger.Ultimate:
                {
                    var elements = new List<VisualElement>()
                    {
                        _ultimateDisplay.cooldownLabel,
                        _ultimateDisplay.abilityIconOverlay,
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
                        _primaryDisplay.cooldownLabel,
                        _primaryDisplay.abilityIconOverlay,
                    };
                    CheckAndApplyDisplayStyle(elements, show);
                    DisplayCooldownLabelTimerAndOverlay(_primaryDisplay, timer, percent);
                }
                    break;

                case AbilityTrigger.Secondary:
                {
                    var elements = new List<VisualElement>()
                    {
                        _secondaryDisplay.cooldownLabel,
                        _secondaryDisplay.abilityIconOverlay,
                    };
                    CheckAndApplyDisplayStyle(elements, show);
                    DisplayCooldownLabelTimerAndOverlay(_secondaryDisplay, timer, percent);
                }
                    break;

                case AbilityTrigger.Tertiary:
                {
                    var elements = new List<VisualElement>()
                    {
                        _tertiaryDisplay.cooldownLabel,
                        _tertiaryDisplay.abilityIconOverlay,
                    };
                    CheckAndApplyDisplayStyle(elements, show);
                    DisplayCooldownLabelTimerAndOverlay(_tertiaryDisplay, timer, percent);
                }
                    break;

                case AbilityTrigger.Ultimate:
                {
                    var elements = new List<VisualElement>()
                    {
                        _ultimateDisplay.cooldownLabel,
                        _ultimateDisplay.abilityIconOverlay,
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
                    _primaryDisplay.abilityTriggerLabel.text = displayString;
                }
                    break;

                case AbilityTrigger.Secondary:
                {
                    var displayString = InputManager.Instance.PlayerInput.AbilitySecondary.GetBindingDisplayString();
                    _secondaryDisplay.abilityTriggerLabel.text = displayString;
                }
                    break;

                case AbilityTrigger.Tertiary:
                {
                    var displayString = InputManager.Instance.PlayerInput.AbilityTertiary.GetBindingDisplayString();
                    _tertiaryDisplay.abilityTriggerLabel.text = displayString;
                }
                    break;

                case AbilityTrigger.Ultimate:
                {
                    var displayString = InputManager.Instance.PlayerInput.AbilityUltimate.GetBindingDisplayString();
                    _ultimateDisplay.abilityTriggerLabel.text = displayString;
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
                    _primaryDisplay.stackCountLabel.text = stackCount.ToString();
                    _primaryDisplay.stackCountLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                }
                    break;

                case AbilityTrigger.Secondary:
                {
                    _secondaryDisplay.stackCountLabel.text = stackCount.ToString();
                    _secondaryDisplay.stackCountLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                }
                    break;

                case AbilityTrigger.Tertiary:
                {
                    _tertiaryDisplay.stackCountLabel.text = stackCount.ToString();
                    _tertiaryDisplay.stackCountLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                }
                    break;

                case AbilityTrigger.Ultimate:
                {
                    _ultimateDisplay.stackCountLabel.text = stackCount.ToString();
                    _ultimateDisplay.stackCountLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
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
                    _primaryDisplay.abilityBackground.style.backgroundImage = new StyleBackground(abilityBackground);
                    _primaryDisplay.abilityBackground.style.unityBackgroundImageTintColor = backgroundTint;
                    break;

                case AbilityTrigger.Secondary:
                    _secondaryDisplay.abilityBackground.style.backgroundImage = new StyleBackground(abilityBackground);
                    _secondaryDisplay.abilityBackground.style.unityBackgroundImageTintColor = backgroundTint;
                    break;

                case AbilityTrigger.Tertiary:
                    _tertiaryDisplay.abilityBackground.style.backgroundImage = new StyleBackground(abilityBackground);
                    _tertiaryDisplay.abilityBackground.style.unityBackgroundImageTintColor = backgroundTint;
                    break;

                case AbilityTrigger.Ultimate:
                    _ultimateDisplay.abilityBackground.style.backgroundImage = new StyleBackground(abilityBackground);
                    _ultimateDisplay.abilityBackground.style.unityBackgroundImageTintColor = backgroundTint;
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
                    _primaryDisplay.abilityIcon.style.backgroundImage = new StyleBackground(abilityIcon);
                    _primaryDisplay.abilityIconOverlay.style.backgroundImage = new StyleBackground(abilityIcon);
                }
                    break;

                case AbilityTrigger.Secondary:
                {
                    _secondaryDisplay.abilityIcon.style.backgroundImage = new StyleBackground(abilityIcon);
                    _secondaryDisplay.abilityIconOverlay.style.backgroundImage = new StyleBackground(abilityIcon);
                }
                    break;

                case AbilityTrigger.Tertiary:
                {
                    _tertiaryDisplay.abilityIcon.style.backgroundImage = new StyleBackground(abilityIcon);
                    _tertiaryDisplay.abilityIconOverlay.style.backgroundImage = new StyleBackground(abilityIcon);
                }
                    break;

                case AbilityTrigger.Ultimate:
                {
                    _ultimateDisplay.abilityIcon.style.backgroundImage = new StyleBackground(abilityIcon);
                    _ultimateDisplay.abilityIconOverlay.style.backgroundImage = new StyleBackground(abilityIcon);
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
            public VisualElement itemRoot;
            public Label cooldownLabel;
            public Label stackCountLabel;
            public VisualElement abilityBackground;
            public VisualElement abilityIcon;
            public VisualElement abilityIconOverlay;
            public Label abilityTriggerLabel;
        }

        #endregion Structs
    }
}