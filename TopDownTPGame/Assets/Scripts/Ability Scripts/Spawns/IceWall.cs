using UnityEngine;

namespace AbilityScripts.Spawns
{
    public class IceWall : MonoBehaviour
    {
        [Header("Spawn Data")]
        [SerializeField] private float _moveYDistance;
        [SerializeField] private float _spawnDuration;
        [SerializeField] private AnimationCurve _spawnEasing;

        [Header("Destroy Data")]
        [SerializeField] private float _destroyDuration;

        private float _currentDuration;
        private Vector3 _startPosition;
        private Vector3 _endPosition;

        private bool _spawnActive;
        private float _destroyTimeLeft;

        #region Unity Functions

        private void Start()
        {
            _currentDuration = 0;
            _startPosition = transform.position;
            _endPosition = transform.position + Vector3.up * _moveYDistance;

            _spawnActive = true;
            _destroyTimeLeft = _destroyDuration;
        }

        private void FixedUpdate()
        {
            _destroyTimeLeft -= Time.fixedDeltaTime;
            if (_destroyTimeLeft < 0)
            {
                Destroy(gameObject);
                return;
            }

            if (!_spawnActive)
            {
                return;
            }

            float percent = _currentDuration / _spawnDuration;
            float mappedPercent = _spawnEasing.Evaluate(percent);
            Vector3 mappedPosition = Vector3.Lerp(_startPosition, _endPosition, mappedPercent);

            transform.position = mappedPosition;
            _currentDuration += Time.fixedDeltaTime;

            if (_currentDuration > _spawnDuration)
            {
                _spawnActive = false;
            }
        }

        #endregion Unity Functions
    }
}