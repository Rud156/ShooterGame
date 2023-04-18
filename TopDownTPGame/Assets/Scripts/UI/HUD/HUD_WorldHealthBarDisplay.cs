#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Misc;

#endregion

namespace UI.HUD
{
    public class HUD_WorldHealthBarDisplay : MonoBehaviour
    {
        private const string HealthBarParentString = "UI_WorldHealthBarContainer";

        private VisualElement _parent;
        private List<HealthBarData> _healthBars;
        private int _healthBarCounterId;

        #region Utils

        #region General

        public int GetHealthBarIndexFromId(int healthBarId)
        {
            for (var i = 0; i < _healthBars.Count; i++)
            {
                if (_healthBars[i].healthBarId == healthBarId)
                {
                    return i;
                }
            }

            return -1;
        }

        private void Initialize()
        {
            var root = GameObject.FindWithTag(TagManager.UIRoot).GetComponent<UIDocument>().rootVisualElement;
            _parent = root.Q<VisualElement>(HealthBarParentString);
            _healthBars = new List<HealthBarData>();
            _healthBarCounterId = 0;
        }

        #endregion General

        #region External Functions

        public int CreateHealthBar(Vector3 position, int currentHealth, int maxHealth)
        {
            throw new NotImplementedException();
        }

        public void UpdateHealthBar(int healthBarId, Vector3 position, int currentHealth, int maxHealth)
        {
        }

        public void DestroyHealthBar(int healthBarId)
        {
        }

        #endregion External Functions

        #endregion Utils

        #region Structs

        private struct HealthBarData
        {
            public int healthBarId;
            public VisualElement healthBarWidget;
            public Label healthBarLabel;
        }

        #endregion Structs

        #region Singleton

        public static HUD_WorldHealthBarDisplay Instance { get; private set; }

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