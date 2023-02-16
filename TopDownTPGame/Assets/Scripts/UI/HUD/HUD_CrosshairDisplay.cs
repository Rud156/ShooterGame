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

        private const float CenterDotInitialOffset = 35;
        private const float CenterDotOffsetPerPixel = 0.5f;

        private VisualElement _centerDot;
        private VisualElement _leftLine;
        private VisualElement _rightLine;
        private VisualElement _topLine;
        private VisualElement _bottomLine;

        [Header("Lines")]
        [SerializeField] [Range(0, 100)] private float _crosshairOffset;
        [SerializeField] [Range(0, 100)] private float _crosshairThickness;
        [SerializeField] [Range(0, 100)] private float _crosshairLength;
        [SerializeField] [Range(0, 1)] private float _crosshairAlpha;
        [SerializeField] private Color _crosshairColor = Colors.Black;
        [SerializeField] [Range(0, 1)] private float _crosshairOutlineThickness;
        [SerializeField] [Range(0, 1)] private float _crosshairOutlineAlpha;

        [Header("Center Dot")]
        [SerializeField] [Range(0, 100)] private float _centerDotThickness;
        [SerializeField] [Range(0, 1)] private float _centerDotAlpha;
        [SerializeField] private Color _centerDotColor = Colors.Black;
        [SerializeField] [Range(0, 100)] private float _centerDotOutlineThickness;
        [SerializeField] [Range(0, 1)] private float _centerDotOutlineAlpha;
        [SerializeField] private Color _centerDotOutlineColor = Colors.Black;

        private bool _isInitialized;

        #region Unity Functions

        private void Start()
        {
            UpdateCenterDot();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying || !_isInitialized)
            {
                return;
            }

            UpdateCenterDot();
        }

        #endregion Unity Functions

        #region Utils

        #region Center Dot

        private void UpdateCenterDot()
        {
            _centerDot.style.width = _centerDotThickness;
            _centerDot.style.height = _centerDotThickness;

            var mappedOffset = _centerDotThickness * CenterDotOffsetPerPixel;
            var finalOffset = CenterDotInitialOffset - mappedOffset;
            _centerDot.style.left = finalOffset;
            _centerDot.style.top = finalOffset;

            _centerDot.style.borderLeftWidth = _centerDotOutlineThickness;
            _centerDot.style.borderRightWidth = _centerDotOutlineThickness;
            _centerDot.style.borderTopWidth = _centerDotOutlineThickness;
            _centerDot.style.borderBottomWidth = _centerDotOutlineThickness;

            var centerDotColor = new Color(_centerDotColor.r, _centerDotColor.g, _centerDotColor.b, _centerDotAlpha);
            _centerDot.style.unityBackgroundImageTintColor = centerDotColor;

            var centerDotOutlineColor = new Color(_centerDotOutlineColor.r, _centerDotOutlineColor.g, _centerDotOutlineColor.b, _centerDotOutlineAlpha);
            _centerDot.style.borderLeftColor = centerDotOutlineColor;
            _centerDot.style.borderRightColor = centerDotOutlineColor;
            _centerDot.style.borderTopColor = centerDotOutlineColor;
            _centerDot.style.borderBottomColor = centerDotOutlineColor;
        }

        #endregion Center Dot

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