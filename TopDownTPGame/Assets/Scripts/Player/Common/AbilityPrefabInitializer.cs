#region

using UnityEngine;
using Utils.Misc;

#endregion

namespace Player.Common
{
    public class AbilityPrefabInitializer : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _abilityPrefab;
        [SerializeField] private GameObject _abilityCameraParentPrefab;

        [Header("Offsets")]
        [SerializeField] private Vector3 _defaultOffset;
        [SerializeField] private Vector3 _cameraParentOffset;

        private Transform _playerCinemachine;
        private GameObject _cameraPrefabInstance;
        private GameObject _defaultPrefabInstance;

        private bool _isInitialized;

        #region Unity Functions

        private void Start() => AbilityPrefabInit();

        private void OnDestroy()
        {
            if (_defaultPrefabInstance != null)
            {
                Destroy(_defaultPrefabInstance);
            }

            if (_cameraPrefabInstance != null)
            {
                Destroy(_cameraPrefabInstance);
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void AbilityPrefabInit()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            _playerCinemachine = GameObject.FindGameObjectWithTag(TagManager.PlayerCinemachineController).transform;

            if (_abilityPrefab != null)
            {
                _defaultPrefabInstance = Instantiate(_abilityPrefab, transform);
                _defaultPrefabInstance.transform.localPosition += _defaultOffset;
            }

            if (_abilityCameraParentPrefab != null)
            {
                _cameraPrefabInstance = Instantiate(_abilityCameraParentPrefab, _playerCinemachine);
                _cameraPrefabInstance.transform.localPosition += _cameraParentOffset;
            }
        }

        #endregion External Functions
    }
}