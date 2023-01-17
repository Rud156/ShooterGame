#region

using UnityEngine;
using UnityEngine.UIElements;
using Utils.Misc;

#endregion

namespace DebugHUD
{
    public class HUD_FPSDisplay : MonoBehaviour
    {
        private const string DebugDisplayString = "DebugSettings";

        [Header("Update Stats")]
        [SerializeField] private float _updateRate;

        private VisualElement _root;

        private VisualElement _debugDisplay;
        private Label _fpsDisplay;

        private int _frameCount;
        private float _accumulatedFPS;
        private float _currentUpdateTime;

        #region Unity Functions

        private void Start()
        {
            _root = GameObject.FindWithTag(TagManager.UIRoot).GetComponent<UIDocument>().rootVisualElement;
            _debugDisplay = _root.Q<VisualElement>(DebugDisplayString);
            _fpsDisplay = _debugDisplay.Q<Label>("FPSDisplay");

            _currentUpdateTime = 0;
        }

        private void Update()
        {
            _frameCount += 1;
            var fps = 1.0f / Time.deltaTime;
            _accumulatedFPS += fps;

            _currentUpdateTime -= Time.deltaTime;
            if (_currentUpdateTime <= 0)
            {
                _currentUpdateTime = _updateRate;
                _fpsDisplay.text = $"FPS: {_accumulatedFPS / _frameCount}";

                _accumulatedFPS = 0;
                _frameCount = 0;
            }
        }

        #endregion Unity Functions
    }
}