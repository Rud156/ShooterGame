#region

using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace UI
{
    public class PlayerHealthDisplay : MonoBehaviour
    {
        private const string PlayerHealthBar = "PlayerHealth";
        private const string CurrentHealthString = "CurrentHealth";
        private const string MaxHealthString = "MaxHealth";

        [Header("Debug")]
        [SerializeField] [Range(0, 100)] private int _progressValue;

        private VisualElement _root;

        private ProgressBar _healthBar;
        private Label _currentHealthLabel;
        private Label _maxHealthLabel;

        #region Unity Functions

        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _healthBar = _root.Q<ProgressBar>(PlayerHealthBar);
            _currentHealthLabel = _root.Q<Label>(CurrentHealthString);
            _maxHealthLabel = _root.Q<Label>(MaxHealthString);

            var progressBar = _healthBar.Q<VisualElement>("unity-progress-bar");
            var progressBackground = progressBar.Q<VisualElement>(className: "unity-progress-bar__background");

            var titleContainer = progressBackground.Q<VisualElement>(className: "unity-progress-bar__title-container");
            var innerText = titleContainer.Q<Label>(className: "unity-progress-bar__title");
            innerText.text = string.Empty;
        }

        private void Update()
        {
            _healthBar.value = _progressValue;
            _currentHealthLabel.text = _progressValue.ToString();
            _maxHealthLabel.text = $" / {_progressValue.ToString()}";
        }

        #endregion Unity Functions
    }
}