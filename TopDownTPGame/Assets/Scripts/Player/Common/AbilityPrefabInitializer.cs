#region

using UnityEngine;

#endregion

namespace Player.Common
{
    public class AbilityPrefabInitializer : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _abilityPrefab;
        [SerializeField] private GameObject _abilityCameraParentPrefab;
        [SerializeField] private GameObject _abilityCameraHolderPrefab;

        [Header("Components")]
        [SerializeField] private Transform _cameraHolder;
        [SerializeField] private Transform _camera;

        [Header("Offsets")]
        [SerializeField] private Vector3 _defaultOffset;
        [SerializeField] private Vector3 _cameraParentOffset;
        [SerializeField] private Vector3 _cameraHolderOffset;

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
                var instance = Instantiate(_abilityPrefab, transform);
                instance.transform.localPosition += _defaultOffset;
            }

            if (_abilityCameraParentPrefab != null)
            {
                var instance = Instantiate(_abilityCameraParentPrefab, _camera);
                instance.transform.localPosition += _cameraParentOffset;
            }

            if (_abilityCameraHolderPrefab != null)
            {
                var instance = Instantiate(_abilityCameraHolderPrefab, _cameraHolder);
                instance.transform.localPosition += _cameraHolderOffset;
            }
        }

        #endregion External Functions
    }
}