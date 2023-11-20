using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Misc;

namespace UI.Telemetry
{
    public class HUD_TelemetryDisplay : MonoBehaviour
    {
        private const string TelemetryContainerString = "UI_TelemetrySettings";
        private const string TelemetrySettingsGridString = "TelemetrySettingsGrid";
        private const string GridItemString = "Item_";
        private const string LabelString = "Label";

        [Header("Display Count")]
        [SerializeField] private int _telemetryGridItemCount;
        [SerializeField] private List<TelemetryDisplayType> _telemetryDisplayTypes;

        [Header("FPS Update Data")]
        [SerializeField] private float _fpsUpdateRate;

        [Header("Time Data")]
        [SerializeField] private TimeFormat _timeFormat;

        // All Items
        private List<GridItem> _gridItems;

        // FPS Display
        private int _fpsFrameCount;
        private float _accumulatedFps;
        private float _fpsCurrentUpdateTime;

        #region Unity Functions

        private void Start()
        {
            _gridItems = new List<GridItem>();

            var root = GameObject.FindWithTag(TagManager.UIRoot).GetComponent<UIDocument>().rootVisualElement;
            var debugSettingsContainer = root.Q<VisualElement>(TelemetryContainerString);
            var debugSettingsGrid = debugSettingsContainer.Q<VisualElement>(TelemetrySettingsGridString);

            for (var i = 1; i <= _telemetryGridItemCount; i++)
            {
                var item = debugSettingsGrid.Q<VisualElement>($"{GridItemString}{i}");
                var label = item.Q<Label>(LabelString);

                _gridItems.Add(new GridItem()
                {
                    DisplayParent = item,
                    DisplayLabel = label
                });
            }
        }

        private void Update()
        {
            var displayItemsCount = _telemetryDisplayTypes.Count;
            for (var i = 0; i < _telemetryGridItemCount; i++)
            {
                _gridItems[i].DisplayParent.style.display = i >= displayItemsCount ? DisplayStyle.None : DisplayStyle.Flex;
            }

            for (var i = 0; i < _telemetryDisplayTypes.Count; i++)
            {
                var debugDisplay = _telemetryDisplayTypes[i];
                var displayLabel = _gridItems[i].DisplayLabel;
                switch (debugDisplay)
                {
                    case TelemetryDisplayType.FPS:
                        DisplayFPSCounter(displayLabel);
                        break;

                    case TelemetryDisplayType.Clock:
                        DisplayClock(displayLabel);
                        break;

                    case TelemetryDisplayType.Ping:
                        DisplayPlayerPing(displayLabel);
                        break;

                    case TelemetryDisplayType.PacketLossTotalPercent:
                        DisplayPacketLossPercent(displayLabel);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion Unity Functions

        #region Utils

        private void DisplayFPSCounter(TextElement element)
        {
            _fpsFrameCount += 1;
            var fps = 1.0f / Time.deltaTime;
            _accumulatedFps += fps;

            _fpsCurrentUpdateTime -= Time.deltaTime;
            if (_fpsCurrentUpdateTime <= 0)
            {
                _fpsCurrentUpdateTime = _fpsUpdateRate;
                var computedFps = _accumulatedFps / _fpsFrameCount;
                element.text = $"FPS: {computedFps:0.00}";

                _accumulatedFps = 0;
                _fpsFrameCount = 0;
            }
        }

        private void DisplayClock(TextElement element)
        {
            var dateTime = _timeFormat switch
            {
                TimeFormat.Hr24 => DateTime.Now.ToString("HH:mm"),
                TimeFormat.Hr12 => DateTime.Now.ToString("h:mm tt"),
                _ => throw new ArgumentOutOfRangeException()
            };

            element.text = $"Time: {dateTime}";
        }

        private void DisplayPlayerPing(TextElement element)
        {
        }

        private void DisplayPacketLossPercent(TextElement element)
        {
        }

        #endregion Utils

        #region Structs

        private struct GridItem
        {
            public VisualElement DisplayParent;
            public Label DisplayLabel;
        }

        #endregion Structs

        #region Enums

        private enum TelemetryDisplayType
        {
            FPS,
            Clock,
            Ping,
            PacketLossTotalPercent,
        }

        private enum TimeFormat
        {
            Hr24,
            Hr12,
        }

        #endregion Enums
    }
}