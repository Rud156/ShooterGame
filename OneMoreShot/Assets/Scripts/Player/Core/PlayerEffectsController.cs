using System;
using UnityEngine;

namespace Player.Core
{
    public class PlayerEffectsController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private EffectSpawnData _jumpEffect;
        [SerializeField] private EffectSpawnData _runEffect;
        [SerializeField] private EffectSpawnData _landEffect;

        [Header("Components")]
        [SerializeField] private PlayerController _playerController;

        private ParticleSystem _runEffectInstance;

        #region Unity Functions

        private void Start()
        {
            _playerController.OnPlayerJumped += HandlePlayerJumped;
            _playerController.OnPlayerGroundedChanged += HandlePlayerGroundedStateChanged;

            var runEffectObject = Instantiate(_runEffect.effectPrefab, transform.position, Quaternion.Euler(_runEffect.spawnRotation));
            runEffectObject.transform.SetParent(transform);
            runEffectObject.transform.localPosition += _runEffect.spawnOffset;
            _runEffectInstance = runEffectObject.GetComponent<ParticleSystem>();
            _runEffectInstance.Stop();
        }

        private void OnDestroy()
        {
            _playerController.OnPlayerJumped -= HandlePlayerJumped;
            _playerController.OnPlayerGroundedChanged -= HandlePlayerGroundedStateChanged;
        }

        private void Update() => UpdatePlayerStateChanged();

        #endregion Unity Functions

        #region Effects State Handling

        private void HandlePlayerJumped()
        {
            var jumpEffectObject = Instantiate(_jumpEffect.effectPrefab, transform.position, Quaternion.Euler(_jumpEffect.spawnRotation));
            jumpEffectObject.transform.SetParent(transform);
            jumpEffectObject.transform.localPosition += _jumpEffect.spawnOffset;
            jumpEffectObject.transform.SetParent(null);
        }

        private void HandlePlayerGroundedStateChanged(bool previousState, bool newState)
        {
            if (!newState)
            {
                return;
            }

            var topPlayerState = _playerController.TopPlayerState;
            if (topPlayerState == PlayerState.CustomMovement)
            {
                return;
            }

            var landEffectObject = Instantiate(_landEffect.effectPrefab, transform.position, Quaternion.Euler(_landEffect.spawnRotation));
            landEffectObject.transform.SetParent(transform);
            landEffectObject.transform.localPosition += _landEffect.spawnOffset;
            landEffectObject.transform.SetParent(null);
        }

        private void UpdatePlayerStateChanged()
        {
            var playerTopState = _playerController.TopPlayerState;
            switch (playerTopState)
            {
                case PlayerState.Idle:
                case PlayerState.Falling:
                case PlayerState.Dead:
                case PlayerState.CustomMovement:
                    {
                        if (_runEffectInstance.isPlaying)
                        {
                            _runEffectInstance.Stop();
                        }
                    }

                    break;

                case PlayerState.Running:
                    {
                        if (!_runEffectInstance.isPlaying)
                        {
                            _runEffectInstance.Play();
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(playerTopState), playerTopState, null);
            }
        }

        #endregion Effects State Handling

        #region Structs

        [Serializable]
        private struct EffectSpawnData
        {
            public Vector3 spawnOffset;
            public Vector3 spawnRotation;
            public GameObject effectPrefab;
        }

        #endregion Structs
    }
}