#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Misc;

#endregion

namespace UI.Debug
{
    public class HUD_DebugDisplayController : MonoBehaviour
    {
        private const string DebugContainerString = "UI_DebugSettings";
        private const string DebugSettingsGridString = "DebugSettingsGrid";
        private const string GridItemString = "Item_";
        private const string LabelString = "Label";

        [Header("Display Count")]
        [SerializeField] private int _debugGridItemCount;
        [SerializeField] private List<DebugDisplayType> _debugDisplayTypes;

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
            var debugSettingsContainer = root.Q<VisualElement>(DebugContainerString);
            var debugSettingsGrid = debugSettingsContainer.Q<VisualElement>(DebugSettingsGridString);

            for (var i = 1; i <= _debugGridItemCount; i++)
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
            var displayItemsCount = _debugDisplayTypes.Count;
            for (var i = 0; i < _debugGridItemCount; i++)
            {
                _gridItems[i].DisplayParent.style.display = i >= displayItemsCount ? DisplayStyle.None : DisplayStyle.Flex;
            }

            for (var i = 0; i < _debugDisplayTypes.Count; i++)
            {
                var debugDisplay = _debugDisplayTypes[i];
                var displayLabel = _gridItems[i].DisplayLabel;
                switch (debugDisplay)
                {
                    case DebugDisplayType.FPS:
                        DisplayFPSCounter(displayLabel);
                        break;

                    case DebugDisplayType.Clock:
                        DisplayClock(displayLabel);
                        break;

                    case DebugDisplayType.Ping:
                        DisplayPlayerPing(displayLabel);
                        break;

                    case DebugDisplayType.PacketLossTotalPercent:
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

        private enum DebugDisplayType
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