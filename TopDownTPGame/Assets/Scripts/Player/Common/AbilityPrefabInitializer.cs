using UnityEngine;

namespace Player.Common
{
    public class AbilityPrefabInitializer : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _abilityPrefab;
        [SerializeField] private GameObject _abilityCameraParentPrefab;

        [Header("Components")]
        [SerializeField] private Transform _cameraHolder;

        private bool _isInitialized;

        #region Unity Functions

        private void Start() => AbilityPrefabInit();

        #endregion Unity Functions

        #region External Functions

        public void AbilityPrefabInit()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            if (_abilityPrefab != null)
            {
                Instantiate(_abilityPrefab, transform);
            }

            if (_abilityCameraParentPrefab != null)
            {
                Instantiate(_abilityCameraParentPrefab, _cameraHolder);
            }
        }

        #endregion External Functions
    }
}