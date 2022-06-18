using UnityEngine;

namespace Common
{
    [RequireComponent(typeof(Renderer))]
    public class DissolveShader : MonoBehaviour
    {
        private static readonly int AlphaClipId = Shader.PropertyToID("_AlphaClip");

        [SerializeField] private float m_lerpSpeed;

        private Material m_material;

        private float m_lerpStart;
        private float m_lerpEnd;
        private float m_lerpAmount;

        #region Unity Functions

        private void Start()
        {
            m_material = GetComponent<Renderer>().material;
            m_lerpAmount = 1;
        }

        private void Update()
        {
            if (m_lerpAmount >= 1)
            {
                return;
            }

            m_lerpAmount += Time.deltaTime * m_lerpSpeed;
            float lerpedValue = Mathf.Lerp(m_lerpStart, m_lerpEnd, m_lerpAmount);

            m_material.SetFloat(AlphaClipId, lerpedValue);
        }

        #endregion

        #region External Functions

        public void SetDissolve(bool active)
        {
            m_lerpAmount = 0;
            if (active)
            {
                m_lerpStart = 0;
                m_lerpEnd = 1;
            }
            else
            {
                m_lerpStart = 1;
                m_lerpEnd = 0;
            }
        }

        #endregion
    }
}