#region

using UnityEngine;
using UnityEngine.UIElements;
using Utils.Misc;

#endregion

namespace UI.HUD
{
    public class HUD_CrosshairDisplay : MonoBehaviour
    {
        private const string CrosshairParentString = "UI_Crosshair";
        private const string CenterDotString = "CenterDot";
        private const string LeftLineString = "Left";
        private const string RightLineString = "Right";
        private const string TopLineString = "Top";
        private const string BottomLineString = "Bottom";

        private const int DefaultLineWidth = 10;
        private const int DefaultLineHeight = 5;
        private const int CenterDotDefaultSize = 10;

        private const float LeftLineDefaultXOffset = -10;
        private const float RightLineDefaultXOffset = 0;
        private const float HorizontalLineDefaultYOffset = -2.5f;

        private const float TopLineDefaultYOffset = -10;
        private const float BottomLineDefaultYOffset = 0;
        private const float VerticalLineDefaultXOffset = -2.5f;

        private VisualElement _centerDot;
        private VisualElement _leftLine;
        private VisualElement _rightLine;
        private VisualElement _topLine;
        private VisualElement _bottomLine;

        [Header("Lines")]
        [SerializeField] [Range(0, 25)] private float _horizontalLineOffset;
        [SerializeField] [Range(0, 25)] private float _verticalLineOffset;
        [SerializeField] [Range(0, 25)] private float _horizontalLineThickness;
        [SerializeField] [Range(0, 25)] private float _verticalLineThickness;
        [SerializeField] [Range(0, 25)] private float _horizontalLineLength;
        [SerializeField] [Range(0, 25)] private float _verticalLineLength;
        [SerializeField] [Range(0, 1)] private float _crosshairAlpha;
        [SerializeField] private Color _crosshairColor = Colors.Black;
        [SerializeField] [Range(0, 1)] private float _crosshairOutlineThickness;
        [SerializeField] [Range(0, 1)] private float _crosshairOutlineAlpha;
        [SerializeField] private Color _crosshairOutlineColor = Colors.Black;

        [Header("Center Dot")]
        [SerializeField] [Range(0, 25)] private float _centerDotThickness;
        [SerializeField] [Range(0, 1)] private float _centerDotAlpha;
        [SerializeField] private Color _centerDotColor = Colors.Black;
        [SerializeField] [Range(0, 1)] private float _centerDotOutlineThickness;
        [SerializeField] [Range(0, 1)] private float _centerDotOutlineAlpha;
        [SerializeField] private Color _centerDotOutlineColor = Colors.Black;

        private bool _isInitialized;

        #region Unity Functions

        private void Start() => UpdateCrosshair();

        private void OnValidate()
        {
            if (!Application.isPlaying || !_isInitialized)
            {
                return;
            }

            UpdateCrosshair();
        }

        #endregion Unity Functions

        #region Utils

        #region General

        private void UpdateCrosshair()
        {
            UpdateLinesColorAndAlpha();
            UpdateHorizontalLineLengthAndPosition();
            UpdateVerticalLineLengthAndPosition();
            UpdateCenterDot();
        }

        private void SetElementBorderColor(VisualElement element, Color color)
        {
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
        }

        private void SetElementBorderThickness(VisualElement element, float thickness, float length)
        {
            element.style.borderLeftWidth = length;
            element.style.borderRightWidth = length;
            element.style.borderTopWidth = thickness;
            element.style.borderBottomWidth = thickness;
        }

        private void Initialize()
        {
            var root = GameObject.FindWithTag(TagManager.UIRoot).GetComponent<UIDocument>().rootVisualElement;
            var parent = root.Q<VisualElement>(CrosshairParentString);

            _centerDot = parent.Q<VisualElement>(CenterDotString);
            _leftLine = parent.Q<VisualElement>(LeftLineString);
            _rightLine = parent.Q<VisualElement>(RightLineString);
            _topLine = parent.Q<VisualElement>(TopLineString);
            _bottomLine = parent.Q<VisualElement>(BottomLineString);

            _isInitialized = true;
        }

        #endregion General

        #region Lines

        private void UpdateLinesColorAndAlpha()
        {
            var lineColor = new Color(_crosshairColor.r, _crosshairColor.g, _crosshairColor.b, _crosshairAlpha);
            _leftLine.style.unityBackgroundImageTintColor = lineColor;
            _rightLine.style.unityBackgroundImageTintColor = lineColor;
            _topLine.style.unityBackgroundImageTintColor = lineColor;
            _bottomLine.style.unityBackgroundImageTintColor = lineColor;

            var outlineColor = new Color(_crosshairOutlineColor.r, _crosshairOutlineColor.g, _crosshairOutlineColor.b, _crosshairOutlineAlpha);
            SetElementBorderColor(_leftLine, outlineColor);
            SetElementBorderColor(_rightLine, outlineColor);
            SetElementBorderColor(_topLine, outlineColor);
            SetElementBorderColor(_bottomLine, outlineColor);
        }

        private void UpdateHorizontalLineLengthAndPosition()
        {
            // Set Scale for Left and Right Lines
            _leftLine.style.scale = new Vector2(_horizontalLineLength, _horizontalLineThickness);
            _rightLine.style.scale = new Vector2(_horizontalLineLength, _horizontalLineThickness);

            // Update X and Y Positions for Left and Right Line
            _leftLine.style.translate = new StyleTranslate(new Translate(LeftLineDefaultXOffset - _horizontalLineOffset, HorizontalLineDefaultYOffset));
            _rightLine.style.translate = new StyleTranslate(new Translate(RightLineDefaultXOffset + _horizontalLineOffset, HorizontalLineDefaultYOffset));

            // Set Outlines for Left and Right Lines
            var mappedLeftRightOutlineLength = ExtensionFunctions.Map(_crosshairOutlineThickness, 0, 1, 0, DefaultLineWidth / 2.0f);
            var mappedLeftRightOutlineThickness = ExtensionFunctions.Map(_crosshairOutlineThickness, 0, 1, 0, DefaultLineHeight / 2.0f);
            var clampedOutlineLength = Mathf.CeilToInt(mappedLeftRightOutlineLength);
            var clampedOutlineThickness = Mathf.CeilToInt(mappedLeftRightOutlineThickness);
            SetElementBorderThickness(_leftLine, clampedOutlineThickness, clampedOutlineLength);
            SetElementBorderThickness(_rightLine, clampedOutlineThickness, clampedOutlineLength);
        }

        private void UpdateVerticalLineLengthAndPosition()
        {
            // Set Size for Top and Bottom Lines
            _topLine.style.scale = new Vector2(_verticalLineLength, _verticalLineThickness);
            _bottomLine.style.scale = new Vector2(_verticalLineLength, _verticalLineThickness);

            // Update X and Y Positions for Top and Bottom Lines
            _topLine.style.translate = new StyleTranslate(new Translate(VerticalLineDefaultXOffset, TopLineDefaultYOffset - _verticalLineOffset));
            _bottomLine.style.translate = new StyleTranslate(new Translate(VerticalLineDefaultXOffset, BottomLineDefaultYOffset + _verticalLineOffset));

            // Set Outlines for Top and Bottom Lines
            var mappedTopBottomOutlineLength = ExtensionFunctions.Map(_crosshairOutlineThickness, 0, 1, 0, DefaultLineHeight / 2.0f);
            var mappedTopBottomOutlineThickness = ExtensionFunctions.Map(_crosshairOutlineThickness, 0, 1, 0, DefaultLineWidth / 2.0f);
            var clampedOutlineLength = Mathf.CeilToInt(mappedTopBottomOutlineLength);
            var clampedOutlineThickness = Mathf.CeilToInt(mappedTopBottomOutlineThickness);
            SetElementBorderThickness(_topLine, clampedOutlineLength, clampedOutlineThickness);
            SetElementBorderThickness(_bottomLine, clampedOutlineLength, clampedOutlineThickness);
        }

        #endregion Lines

        #region Center Dot

        private void UpdateCenterDot()
        {
            _centerDot.style.scale = Vector2.one * _centerDotThickness;

            var mappedThickness = ExtensionFunctions.Map(_centerDotOutlineThickness, 0, 1, 0, CenterDotDefaultSize / 2.0f);
            var clampedThickness = Mathf.CeilToInt(mappedThickness);
            SetElementBorderThickness(_centerDot, clampedThickness, clampedThickness);

            var centerDotColor = new Color(_centerDotColor.r, _centerDotColor.g, _centerDotColor.b, _centerDotAlpha);
            _centerDot.style.unityBackgroundImageTintColor = centerDotColor;

            var centerDotOutlineColor = new Color(_centerDotOutlineColor.r, _centerDotOutlineColor.g, _centerDotOutlineColor.b, _centerDotOutlineAlpha);
            SetElementBorderColor(_centerDot, centerDotOutlineColor);
        }

        #endregion Center Dot

        #endregion Utils

        #region Singleton

        public static HUD_CrosshairDisplay Instance { get; private set; }

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