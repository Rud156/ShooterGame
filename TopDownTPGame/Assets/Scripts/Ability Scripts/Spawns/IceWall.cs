#region

using HealthSystem;
using UnityEngine;

#endregion

namespace Ability_Scripts.Spawns
{
    public class IceWall : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _destroyEffectPrefab;

        [Header("Components")]
        [SerializeField] private HealthAndDamage _healthAndDamage;

        [Header("Spawn Data")]
        [SerializeField] private float _moveYDistance;
        [SerializeField] private float _spawnDuration;
        [SerializeField] private AnimationCurve _spawnEasing;

        [Header("Destroy Data")]
        [SerializeField] private Color _fullHealthColor;
        [SerializeField] private Color _minHealthColor;
        [SerializeField] private float _destroyDuration;

        private float _currentSpawnDuration;
        private Vector3 _startPosition;
        private Vector3 _endPosition;

        private bool _spawnActive;
        private float _destroyTimeLeft;

        private Material _material;

        #region Unity Functions

        private void Start()
        {
            _healthAndDamage.OnHealthChanged += HandleHealthAndDamage;
            _material = GetComponent<MeshRenderer>().material;

            _currentSpawnDuration = 0;
            var position = transform.position;
            _startPosition = position;
            _endPosition = position + Vector3.up * _moveYDistance;

            _spawnActive = true;
            _destroyTimeLeft = _destroyDuration;
        }

        private void OnDestroy() => _healthAndDamage.OnHealthChanged += HandleHealthAndDamage;

        private void FixedUpdate()
        {
            _destroyTimeLeft -= Time.fixedDeltaTime;
            if (_destroyTimeLeft < 0)
            {
                DestroyWall();
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

        #region Utils

        private void HandleHealthAndDamage(int startHealth, int currentHealth, int maxHealth)
        {
            _material.color = Color.Lerp(_minHealthColor, _fullHealthColor, (float)currentHealth / maxHealth);
            if (currentHealth <= 0)
            {
                DestroyWall();
            }
        }

        private void DestroyWall()
        {
            Instantiate(_destroyEffectPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        #endregion Utils
    }
}