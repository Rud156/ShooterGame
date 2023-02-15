#region

using UnityEngine;
using UnityEngine.UIElements;
using Utils.Misc;

#endregion

namespace UI.Player
{
    public class HUD_PlayerIconDisplay : MonoBehaviour
    {
        private const string PlayerIconParentString = "UI_PlayerIconWidget";
        private const string PlayerIconString = "PlayerIcone";
        private const string PlayerIconBackgroundString = "PlayerIconBackground";

        private const string PlayerOverlayBackerString = "PlayerIconOverlayBacker";
        private const string PlayerIconOverlayString = "PlayerIconOverlay";
        private const string PlayerIconLabelString = "PlayerIconLabel";

        private VisualElement _parent;

        private VisualElement _playerIcon;
        private VisualElement _playerIconBackground;

        #region External Functions

        public void DisplayPlayerIcon(Sprite icon) => _playerIcon.style.backgroundImage = new StyleBackground(icon);

        public void DisplayPlayerBackground(Sprite icon) => _playerIconBackground.style.backgroundImage = _playerIcon.style.backgroundImage = new StyleBackground(icon);

        public void TintPlayerIcon(Color tint) => _playerIcon.style.unityBackgroundImageTintColor = tint;

        public void TintPlayerBackground(Color tint) => _playerIconBackground.style.unityBackgroundImageTintColor = tint;

        public void DisplayOverlayTimer(float percent, float time)
        {
            // TODO: Complete this function...
        }

        #endregion External Functions

        #region Utils

        private void Initialize()
        {
            var root = GameObject.FindWithTag(TagManager.UIRoot).GetComponent<UIDocument>().rootVisualElement;
            _parent = root.Q<VisualElement>(PlayerIconParentString);
            _playerIcon = _parent.Q<VisualElement>(PlayerIconString);
            _playerIconBackground = _parent.Q<VisualElement>(PlayerIconBackgroundString);
        }

        #endregion Utils

        #region Singleton

        public static HUD_PlayerIconDisplay Instance { get; private set; }

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