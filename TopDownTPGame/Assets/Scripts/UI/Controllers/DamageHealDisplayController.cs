#region

using System.Collections.Generic;
using HealthSystem;
using UI.HUD;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

namespace UI.Controllers
{
    [RequireComponent(typeof(HealthAndDamage))]
    public class DamageHealDisplayController : MonoBehaviour
    {
        [Header("World Display")]
        [SerializeField] private Vector3 _labelDisplayOffset;
        [SerializeField] private Vector3 _randomOffsetRange;

        private Camera _mainCamera;
        private HealthAndDamage _healthAndDamage;
        private List<DisplayWidgetData> _widgetData;

        #region Unity Functions

        private void Start()
        {
            _mainCamera = Camera.main;
            _healthAndDamage = GetComponent<HealthAndDamage>();
            _widgetData = new List<DisplayWidgetData>();

            _healthAndDamage.OnDamageTaken += HandleDamageTaken;
            _healthAndDamage.OnHealed += HandleHealed;
        }

        private void OnDestroy()
        {
            _healthAndDamage.OnDamageTaken -= HandleDamageTaken;
            _healthAndDamage.OnHealed -= HandleHealed;
        }

        private void Update()
        {
            for (var i = _widgetData.Count - 1; i >= 0; i--)
            {
                var screenPoint = _mainCamera.WorldToScreenPoint(_widgetData[i].WidgetWorldPosition);
                var mappedPoint = new Vector2(screenPoint.x, Screen.height - screenPoint.y);

                var widgetExists = HUD_DamageHealDisplay.Instance.UpdateWidgetPosition(_widgetData[i].WidgetId, mappedPoint);
                if (!widgetExists)
                {
                    _widgetData.RemoveAt(i);
                }
            }
        }

        #endregion Unity Functions

        #region Utils

        private void HandleDamageTaken(int damageTaken, int startingHealth, int finalHealth)
        {
            var randomOffset = new Vector3(
                Random.Range(-_randomOffsetRange.x, _randomOffsetRange.x),
                Random.Range(-_randomOffsetRange.y, _randomOffsetRange.y),
                Random.Range(-_randomOffsetRange.z, _randomOffsetRange.z)
            );

            var widgetWorldPosition = transform.position + _labelDisplayOffset + randomOffset;
            var screenPoint = _mainCamera.WorldToScreenPoint(widgetWorldPosition);
            var mappedPoint = new Vector2(screenPoint.x, Screen.height - screenPoint.y);

            var widgetId = HUD_DamageHealDisplay.Instance.AddDamageWidget(mappedPoint, damageTaken);
            _widgetData.Add(new DisplayWidgetData
            {
                WidgetId = widgetId,
                WidgetWorldPosition = widgetWorldPosition
            });
        }

        private void HandleHealed(int healAmount, int startingHealth, int finalHealth)
        {
            var randomOffset = new Vector3(
                Random.Range(-_randomOffsetRange.x, _randomOffsetRange.x),
                Random.Range(-_randomOffsetRange.y, _randomOffsetRange.y),
                Random.Range(-_randomOffsetRange.z, _randomOffsetRange.z)
            );

            var widgetWorldPosition = transform.position + _labelDisplayOffset + randomOffset;
            var screenPoint = _mainCamera.WorldToScreenPoint(widgetWorldPosition);
            var mappedPoint = new Vector2(screenPoint.x, Screen.height - screenPoint.y);

            var widgetId = HUD_DamageHealDisplay.Instance.AddHealWidget(mappedPoint, healAmount);
            _widgetData.Add(new DisplayWidgetData
            {
                WidgetId = widgetId,
                WidgetWorldPosition = widgetWorldPosition
            });
        }

        #endregion Utils

        #region Structs

        private struct DisplayWidgetData
        {
            public int WidgetId;
            [FormerlySerializedAs("WidgetRandomOffset")]
            [FormerlySerializedAs("WidgetPosition")]
            public Vector3 WidgetWorldPosition;
        }

        #endregion Structs
    }
}