using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CrosshairController : MonoBehaviour
    {
        [Header("Lines")]
        [SerializeField][Range(0, 100)] private float m_crosshairOffset;
        [SerializeField][Range(0, 100)] private float m_crosshairThickness;
        [SerializeField][Range(0, 100)] private float m_crosshairLength;
        [SerializeField][Range(0, 1)] private float m_crosshairAlpha;
        [SerializeField] private Color m_crosshairColor;
        [SerializeField][Range(0, 1)] private float m_crosshairOutlineThickness;
        [SerializeField][Range(0, 1)] private float m_crosshairOutlineAlpha;

        [Header("Center Dot")]
        [SerializeField][Range(0, 100)] private float m_centerDotThickness;
        [SerializeField][Range(0, 1)] private float m_centerDotAlpha;
        [SerializeField] private Color m_centerDotColor;
        [SerializeField][Range(0, 1)] private float m_centerDotOutlineThickness;
        [SerializeField][Range(0, 1)] private float m_centerDotOutlineAlpha;

        [Header("Components")]
        [SerializeField] private CrosshairData m_crosshairLeft;
        [SerializeField] private CrosshairData m_crosshairRight;
        [SerializeField] private CrosshairData m_crosshairTop;
        [SerializeField] private CrosshairData m_crosshairBottom;
        [SerializeField] private CrosshairData m_crosshairCenterDot;
        [SerializeField] private CrosshairData m_crosshairLeftOutline;
        [SerializeField] private CrosshairData m_crosshairRightOutline;
        [SerializeField] private CrosshairData m_crosshairTopOutline;
        [SerializeField] private CrosshairData m_crosshairBottomOutline;
        [SerializeField] private CrosshairData m_crosshairCenterDotOutline;

        #region Unity Functions

        private void OnValidate()
        {
            UpdateCrosshairColorAndAlpha();
            UpdateCenterDot();
            UpdateCrosshairTransform();
        }

        #endregion Unity Functions

        #region Crosshair Setup

        private void UpdateCrosshairTransform()
        {
            m_crosshairLeftOutline.transform.anchoredPosition = new Vector2(-m_crosshairOffset, 0);
            m_crosshairRightOutline.transform.anchoredPosition = new Vector2(m_crosshairOffset, 0);
            m_crosshairTopOutline.transform.anchoredPosition = new Vector2(0, m_crosshairOffset);
            m_crosshairBottomOutline.transform.anchoredPosition = new Vector2(0, -m_crosshairOffset);

            m_crosshairLeftOutline.transform.sizeDelta = new Vector2(m_crosshairLength, m_crosshairThickness);
            m_crosshairRightOutline.transform.sizeDelta = new Vector2(m_crosshairLength, m_crosshairThickness);
            m_crosshairTopOutline.transform.sizeDelta = new Vector2(m_crosshairThickness, m_crosshairLength);
            m_crosshairBottomOutline.transform.sizeDelta = new Vector2(m_crosshairThickness, m_crosshairLength);

            float mappedLength = Mathf.Lerp(0, m_crosshairLength, 1 - m_crosshairOutlineThickness);
            float mappedThickness = Mathf.Lerp(0, m_crosshairThickness, 1 - m_crosshairOutlineThickness);
            m_crosshairLeft.transform.sizeDelta = new Vector2(mappedLength, mappedThickness);
            m_crosshairRight.transform.sizeDelta = new Vector2(mappedLength, mappedThickness);
            m_crosshairTop.transform.sizeDelta = new Vector2(mappedThickness, mappedLength);
            m_crosshairBottom.transform.sizeDelta = new Vector2(mappedThickness, mappedLength);
        }

        private void UpdateCenterDot()
        {
            m_crosshairCenterDotOutline.transform.sizeDelta = new Vector2(m_centerDotThickness, m_centerDotThickness);

            float mappedThickness = Mathf.Lerp(0, m_centerDotThickness, 1 - m_centerDotOutlineThickness);
            m_crosshairCenterDot.transform.sizeDelta = new Vector2(mappedThickness, mappedThickness);
        }

        private void UpdateCrosshairColorAndAlpha()
        {
            m_crosshairLeft.image.color = new Color(m_crosshairColor.r, m_crosshairColor.g, m_crosshairColor.b, m_crosshairAlpha);
            m_crosshairRight.image.color = new Color(m_crosshairColor.r, m_crosshairColor.g, m_crosshairColor.b, m_crosshairAlpha); ;
            m_crosshairTop.image.color = new Color(m_crosshairColor.r, m_crosshairColor.g, m_crosshairColor.b, m_crosshairAlpha); ;
            m_crosshairBottom.image.color = new Color(m_crosshairColor.r, m_crosshairColor.g, m_crosshairColor.b, m_crosshairAlpha); ;
            m_crosshairCenterDot.image.color = new Color(m_centerDotColor.r, m_centerDotColor.g, m_centerDotColor.b, m_centerDotAlpha);

            m_crosshairLeftOutline.image.color = new Color(0, 0, 0, m_crosshairOutlineAlpha);
            m_crosshairRightOutline.image.color = new Color(0, 0, 0, m_crosshairOutlineAlpha);
            m_crosshairTopOutline.image.color = new Color(0, 0, 0, m_crosshairOutlineAlpha);
            m_crosshairBottomOutline.image.color = new Color(0, 0, 0, m_crosshairOutlineAlpha);
            m_crosshairCenterDotOutline.image.color = new Color(0, 0, 0, m_centerDotOutlineAlpha);
        }

        #endregion Crosshair Setup

        #region Structs

        [System.Serializable]
        public struct CrosshairData
        {
            public RectTransform transform;
            public Image image;
        }

        #endregion Structs
    }
}