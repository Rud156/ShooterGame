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

        [Header("Center Dot")]
        [SerializeField][Range(0, 100)] private float m_centerDotThickness;
        [SerializeField][Range(0, 1)] private float m_centerDotAlpha;
        [SerializeField] private Color m_centerDotColor;

        [Header("Components")]
        [SerializeField] private CrosshairData m_crosshairLeft;
        [SerializeField] private CrosshairData m_crosshairRight;
        [SerializeField] private CrosshairData m_crosshairTop;
        [SerializeField] private CrosshairData m_crosshairBottom;
        [SerializeField] private CrosshairData m_crosshairCenterDot;

        #region Unity Functions

        private void OnValidate()
        {
            UpdateCrosshairColorAndAlpha();
            UpdateCenterDot();
            UpdateCrosshairTransform();
        }

        #endregion

        #region Crosshair Setup

        private void UpdateCrosshairTransform()
        {
            m_crosshairLeft.transform.anchoredPosition = new Vector2(-m_crosshairOffset, 0);
            m_crosshairRight.transform.anchoredPosition = new Vector2(m_crosshairOffset, 0);
            m_crosshairTop.transform.anchoredPosition = new Vector2(0, m_crosshairOffset);
            m_crosshairBottom.transform.anchoredPosition = new Vector2(0, -m_crosshairOffset);

            m_crosshairLeft.transform.sizeDelta = new Vector2(m_crosshairLength, m_crosshairThickness);
            m_crosshairRight.transform.sizeDelta = new Vector2(m_crosshairLength, m_crosshairThickness);
            m_crosshairTop.transform.sizeDelta = new Vector2(m_crosshairThickness, m_crosshairLength);
            m_crosshairBottom.transform.sizeDelta = new Vector2(m_crosshairThickness, m_crosshairLength);
        }

        private void UpdateCenterDot() => m_crosshairCenterDot.transform.sizeDelta = new Vector2(m_centerDotThickness, m_centerDotThickness);

        private void UpdateCrosshairColorAndAlpha()
        {
            m_crosshairLeft.image.color = new Color(m_crosshairColor.r, m_crosshairColor.g, m_crosshairColor.b, m_crosshairAlpha);
            m_crosshairRight.image.color = new Color(m_crosshairColor.r, m_crosshairColor.g, m_crosshairColor.b, m_crosshairAlpha); ;
            m_crosshairTop.image.color = new Color(m_crosshairColor.r, m_crosshairColor.g, m_crosshairColor.b, m_crosshairAlpha); ;
            m_crosshairBottom.image.color = new Color(m_crosshairColor.r, m_crosshairColor.g, m_crosshairColor.b, m_crosshairAlpha); ;
            m_crosshairCenterDot.image.color = new Color(m_centerDotColor.r, m_centerDotColor.g, m_centerDotColor.b, m_centerDotAlpha); ;
        }

        #endregion

        #region Structs

        [System.Serializable]
        public struct CrosshairData
        {
            public RectTransform transform;
            public Image image;
        }

        #endregion
    }
}