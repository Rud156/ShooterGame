using UnityEngine;

namespace Player.Common
{
    public class AbilityPrefabInitializer : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _abilityPrefab;

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
            Instantiate(_abilityPrefab, transform);
        }

        #endregion External Functions
    }
}