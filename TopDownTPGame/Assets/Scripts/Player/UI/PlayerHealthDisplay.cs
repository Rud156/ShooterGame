#region

using System;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace UI
{
    public class PlayerHealthDisplay : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] [Range(0, 100)] private int _progressValue;

        private VisualElement _root;
        private VisualElement _playerHealth;

        private ProgressBar _healthBar;

        #region Unity Functions

        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _playerHealth = _root.Q<VisualElement>("PlayerHealth");
            _healthBar = (ProgressBar)_playerHealth;

            var progressBar = _playerHealth.Q<VisualElement>("unity-progress-bar");
            var progressBackground = progressBar.Q<VisualElement>(className: "unity-progress-bar__background");

            var titleContainer = progressBackground.Q<VisualElement>(className: "unity-progress-bar__title-container");
            var innerText = titleContainer.Q<Label>(className: "unity-progress-bar__title");
            innerText.text = string.Empty;
        }

        private void Update()
        {
        }

        #endregion Unity Functions
    }
}