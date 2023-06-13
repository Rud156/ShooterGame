#region

using System;
using System.Collections;
using Player.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Utils.Input;
using Utils.Misc;

#endregion

namespace UI.DisplayManagers.Player
{
    public class HUD_PlayerAbilityDisplay : MonoBehaviour
    {
        private const string PrimaryDisplayString = "PrimaryAbility";
        private const string SecondaryDisplayString = "SecondaryAbility";
        private const string TertiaryDisplayString = "TertiaryAbility";
        private const string UltimateDisplayString = "UltimateAbility";

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

        private AbilityDisplayItem _primaryDisplay;
        private AbilityDisplayItem _secondaryDisplay;
        private AbilityDisplayItem _tertiaryDisplay;
        private AbilityDisplayItem _ultimateDisplay;

        // Coroutine Data
        private CoroutineData _flashCoroutineData;
        private CoroutineData _scaleCoroutineData;

        #region Unity Functions

        private void Start()
        {
            var deviceGroupName = CustomInputManager.Instance.LastUsedDeviceInputType;
            UpdateAllAbilityTriggers(deviceGroupName);
        }

        private void OnDestroy() => CustomInputManager.Instance.OnLastUsedInputChanged -= UpdateAllAbilityTriggers;

        #endregion Unity Functions

        #region Utils

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

                case AbilityTrigger.Tertiary:
                    _flashCoroutineData.TertiaryActive = true;
                    break;

                case AbilityTrigger.Ultimate:
                    _flashCoroutineData.UltimateActive = true;
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

                case AbilityTrigger.Tertiary:
                    _flashCoroutineData.TertiaryActive = false;
                    break;

                case AbilityTrigger.Ultimate:
                    _flashCoroutineData.UltimateActive = false;
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

                case AbilityTrigger.Tertiary:
                    _scaleCoroutineData.TertiaryActive = true;
                    break;

                case AbilityTrigger.Ultimate:
                    _scaleCoroutineData.UltimateActive = true;
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

                case AbilityTrigger.Tertiary:
                    _scaleCoroutineData.TertiaryActive = false;
                    break;

                case AbilityTrigger.Ultimate:
                    _scaleCoroutineData.UltimateActive = false;
                    break;

                case AbilityTrigger.ExternalAddedAbility:
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityTrigger), abilityTrigger, null);
            }
        }

        private void Initialize()
        {
            var root = GameObject.FindWithTag(TagManager.UIRoot).GetComponent<UIDocument>().rootVisualElement;

            var primaryAbility = root.Q<VisualElement>(PrimaryDisplayString);
            _primaryDisplay = new AbilityDisplayItem()
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
            _secondaryDisplay = new AbilityDisplayItem()
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

            var tertiaryAbility = root.Q<VisualElement>(TertiaryDisplayString);
            _tertiaryDisplay = new AbilityDisplayItem()
            {
                ItemRoot = tertiaryAbility,
                AbilityBackground = tertiaryAbility.Q<VisualElement>("BackgroundImage"),
                AbilityIcon = tertiaryAbility.Q<VisualElement>("AbilityIcon"),
                AbilityIconOverlayBacker = tertiaryAbility.Q<VisualElement>("AbilityIconOverlayBacker"),
                AbilityIconOverlay = tertiaryAbility.Q<VisualElement>("AbilityIconOverlay"),
                AbilityTriggerLabel = tertiaryAbility.Q<Label>("TriggerName"),
                TimerLabel = tertiaryAbility.Q<Label>("Timer"),
                CounterLabel = tertiaryAbility.Q<Label>("Counter"),
                Flasher = tertiaryAbility.Q<VisualElement>("Flasher"),
            };

            var ultimateAbility = root.Q<VisualElement>(UltimateDisplayString);
            _ultimateDisplay = new AbilityDisplayItem()
            {
                ItemRoot = ultimateAbility,
                AbilityBackground = ultimateAbility.Q<VisualElement>("BackgroundImage"),
                AbilityIcon = ultimateAbility.Q<VisualElement>("AbilityIcon"),
                AbilityIconOverlayBacker = ultimateAbility.Q<VisualElement>("AbilityIconOverlayBacker"),
                AbilityIconOverlay = ultimateAbility.Q<VisualElement>("AbilityIconOverlay"),
                AbilityTriggerLabel = ultimateAbility.Q<Label>("TriggerName"),
                TimerLabel = ultimateAbility.Q<Label>("Timer"),
                CounterLabel = ultimateAbility.Q<Label>("Counter"),
                Flasher = ultimateAbility.Q<VisualElement>("Flasher"),
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

        private void UpdateAllAbilityTriggers(CustomInputManager.InputType currentInputGroup)
        {
            var inputGroupString = currentInputGroup == CustomInputManager.InputType.GamePad ? CustomInputManager.GamepadGroupString : CustomInputManager.KeyboardMouseGroupString;
            UpdateAbilityTrigger(AbilityTrigger.Primary, inputGroupString);
            UpdateAbilityTrigger(AbilityTrigger.Secondary, inputGroupString);
            UpdateAbilityTrigger(AbilityTrigger.Tertiary, inputGroupString);
            UpdateAbilityTrigger(AbilityTrigger.Ultimate, inputGroupString);
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

        public void TriggerAbilityFlashAndScale(AbilityTrigger abilityTrigger)
        {
            switch (abilityTrigger)
            {
                case AbilityTrigger.Primary:
                {
                    if (!_flashCoroutineData.PrimaryActive)
                    {
                        StartCoroutine(FlashCoroutine(_primaryDisplay.Flasher, AbilityTrigger.Primary));
                    }

                    if (!_scaleCoroutineData.PrimaryActive)
                    {
                        StartCoroutine(ScaleCoroutine(_primaryDisplay.ItemRoot, AbilityTrigger.Primary));
                    }
                }
                    break;

                case AbilityTrigger.Secondary:
                {
                    if (!_flashCoroutineData.SecondaryActive)
                    {
                        StartCoroutine(FlashCoroutine(_secondaryDisplay.Flasher, AbilityTrigger.Secondary));
                    }

                    if (!_scaleCoroutineData.PrimaryActive)
                    {
                        StartCoroutine(ScaleCoroutine(_secondaryDisplay.ItemRoot, AbilityTrigger.Secondary));
                    }
                }
                    break;

                case AbilityTrigger.Tertiary:
                {
                    if (!_flashCoroutineData.TertiaryActive)
                    {
                        StartCoroutine(FlashCoroutine(_tertiaryDisplay.Flasher, AbilityTrigger.Tertiary));
                    }

                    if (!_scaleCoroutineData.PrimaryActive)
                    {
                        StartCoroutine(ScaleCoroutine(_tertiaryDisplay.ItemRoot, AbilityTrigger.Tertiary));
                    }
                }
                    break;

                case AbilityTrigger.Ultimate:
                {
                    if (!_flashCoroutineData.UltimateActive)
                    {
                        StartCoroutine(FlashCoroutine(_ultimateDisplay.Flasher, AbilityTrigger.Ultimate));
                    }

                    if (!_scaleCoroutineData.PrimaryActive)
                    {
                        StartCoroutine(ScaleCoroutine(_ultimateDisplay.ItemRoot, AbilityTrigger.Ultimate));
                    }
                }
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
            public bool PrimaryActive;
            public bool SecondaryActive;
            public bool TertiaryActive;
            public bool UltimateActive;
        }

        #endregion Structs
    }
}