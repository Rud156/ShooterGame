#region

using UnityEngine;

#endregion

namespace Ability_Scripts.Spawns
{
    public class IceWall : MonoBehaviour
    {
        [Header("Spawn Data")]
        [SerializeField] private float _moveYDistance;
        [SerializeField] private float _spawnDuration;
        [SerializeField] private AnimationCurve _spawnEasing;

        [Header("Destroy Data")]
        [SerializeField] private float _destroyDuration;

        private float _currentSpawnDuration;
        private Vector3 _startPosition;
        private Vector3 _endPosition;

        private bool _spawnActive;
        private float _destroyTimeLeft;

        #region Unity Functions

        private void Start()
        {
            _currentSpawnDuration = 0;
            var position = transform.position;
            _startPosition = position;
            _endPosition = position + Vector3.up * _moveYDistance;

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

            var percent = _currentSpawnDuration / _spawnDuration;
            var mappedPercent = _spawnEasing.Evaluate(percent);
            var mappedPosition = Vector3.Lerp(_startPosition, _endPosition, mappedPercent);

            transform.position = mappedPosition;
            _currentSpawnDuration += Time.fixedDeltaTime;

            if (_currentSpawnDuration > _spawnDuration)
            {
                _spawnActive = false;
            }
        }

        #endregion Unity Functions
    }
}