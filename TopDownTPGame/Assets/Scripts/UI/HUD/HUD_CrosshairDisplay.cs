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

        private const float LeftLineInitialXPosition = 25;
        private const float LeftRightLineInitialYPosition = 35;
        private const float LeftRightLineHeightOffsetPerPixel = 0.5f;
        private const float LeftLineWidthOffsetPerPixel = 1;

        private const float CenterDotInitialYPosition = 35;
        private const float CenterDotOffsetPerPixel = 0.5f;

        private VisualElement _centerDot;
        private VisualElement _leftLine;
        private VisualElement _rightLine;
        private VisualElement _topLine;
        private VisualElement _bottomLine;

        [Header("Lines")]
        [SerializeField] [Range(0, 100)] private float _horizontalLineOffset;
        [SerializeField] [Range(0, 100)] private float _verticalLineOffset;
        [SerializeField] [Range(0, 100)] private float _horizontalLineThickness;
        [SerializeField] [Range(0, 100)] private float _verticalLineThickness;
        [SerializeField] [Range(0, 100)] private float _horizontalLength;
        [SerializeField] [Range(0, 100)] private float _verticalLength;
        [SerializeField] [Range(0, 1)] private float _crosshairAlpha;
        [SerializeField] private Color _crosshairColor = Colors.Black;
        [SerializeField] [Range(0, 1)] private float _crosshairOutlineThickness;
        [SerializeField] [Range(0, 1)] private float _crosshairOutlineAlpha;
        [SerializeField] private Color _crosshairOutlineColor = Colors.Black;

        [Header("Center Dot")]
        [SerializeField] [Range(0, 100)] private float _centerDotThickness;
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
            UpdateLineLengthAndPosition();
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

        private void UpdateLineLengthAndPosition()
        {
            // Set Size for Left and Right Lines
            _leftLine.style.width = _horizontalLength;
            _leftLine.style.height = _horizontalLineThickness;
            _rightLine.style.width = _horizontalLength;
            _rightLine.style.height = _horizontalLineThickness;

            // Update Y Position for Left and Right Lines
            var mappedLeftRightYOffset = _horizontalLineThickness * LeftRightLineHeightOffsetPerPixel;
            var finalLeftRightYOffset = LeftRightLineInitialYPosition - mappedLeftRightYOffset;
            _leftLine.style.top = finalLeftRightYOffset;
            _rightLine.style.top = finalLeftRightYOffset;

            // Update Left Position for Left Line
            var mappedLeftXOffset = _horizontalLength * LeftLineWidthOffsetPerPixel;
            var finalLeftXOffset = LeftLineInitialXPosition - mappedLeftXOffset;
            _leftLine.style.left = finalLeftXOffset;

            // Set Outlines for Left and Right Lines
            var mappedLeftRightOutlineLength = ExtensionFunctions.Map(_crosshairOutlineThickness, 0, 1, 0, _horizontalLength / 2.0f);
            var mappedLeftRightOutlineThickness = ExtensionFunctions.Map(_crosshairOutlineThickness, 0, 1, 0, _horizontalLineThickness / 2.0f);
            SetElementBorderThickness(_leftLine, mappedLeftRightOutlineThickness, mappedLeftRightOutlineLength);
            SetElementBorderThickness(_rightLine, mappedLeftRightOutlineThickness, mappedLeftRightOutlineLength);
        }

        #endregion Lines

        #region Center Dot

        private void UpdateCenterDot()
        {
            _centerDot.style.width = _centerDotThickness;
            _centerDot.style.height = _centerDotThickness;

            var mappedOffset = _centerDotThickness * CenterDotOffsetPerPixel;
            var finalOffset = CenterDotInitialYPosition - mappedOffset;
            _centerDot.style.left = finalOffset;
            _centerDot.style.top = finalOffset;

            var mappedCenterDotOutlineThickness = ExtensionFunctions.Map(_centerDotOutlineThickness, 0, 1, 0, _centerDotThickness / 2.0f);
            SetElementBorderThickness(_centerDot, mappedCenterDotOutlineThickness, mappedCenterDotOutlineThickness);

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