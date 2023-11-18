using UnityEngine;

namespace World
{
    public class WorldTimeManager : MonoBehaviour
    {
        [Header("Fixed Update Rate")]
        [SerializeField] private float _fixedUpdateTime;

        public delegate void WorldCustomFixedUpdate(float fixedUpdateTime);
        public WorldCustomFixedUpdate OnWorldCustomFixedUpdate;

        private float _accumulator;

        #region Unity Functions

        private void Update()
        {
            _accumulator += Time.deltaTime;
            while (_accumulator >= _fixedUpdateTime)
            {
                _accumulator -= _fixedUpdateTime;
                OnWorldCustomFixedUpdate?.Invoke(_fixedUpdateTime);
            }
        }

        #endregion Unity Functions

        #region Getters

        public float FixedUpdateTime => _fixedUpdateTime;

        #endregion Getters

        #region Singleton

        public static WorldTimeManager Instance { get; private set; }

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
        }

        #endregion Singleton
    }
}