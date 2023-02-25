#region

using System;
using UnityEngine;

#endregion

namespace Player.Base
{
    public class PlayerBaseEffectsController : MonoBehaviour
    {
        [Header("Effects Prefab")]
        [SerializeField] private EffectSpawnData _jumpEffectPrefab;
        [SerializeField] private EffectSpawnData _runEffectPrefab;
        [SerializeField] private EffectSpawnData _landEffectPrefab;

        [Header("Components")]
        [SerializeField] private BasePlayerController _playerController;

        private ParticleSystem _runEffect;

        #region Unity Functions

        private void Start()
        {
            _playerController.OnPlayerGroundedChanged += HandlePlayerGroundedChanged;
            _playerController.OnPlayerStateChanged += HandlePlayerStateChanged;
            _playerController.OnPlayerJumped += HandlePlayerJumped;

            var runEffectObject = Instantiate(_runEffectPrefab.effectPrefab, transform.position, Quaternion.identity);
            runEffectObject.transform.SetParent(transform);
            runEffectObject.transform.localPosition += _runEffectPrefab.spawnOffset;
            _runEffect = runEffectObject.GetComponent<ParticleSystem>();
            _runEffect.Stop();
        }

        private void OnDestroy()
        {
            _playerController.OnPlayerGroundedChanged -= HandlePlayerGroundedChanged;
            _playerController.OnPlayerStatePushed -= HandlePlayerStateChanged;
            _playerController.OnPlayerJumped -= HandlePlayerJumped;
        }

        #endregion Unity Functions

        #region Utils

        private void HandlePlayerGroundedChanged(bool previousState, bool newState)
        {
        }

        private void HandlePlayerStateChanged(PlayerState currentState)
        {
            switch (currentState)
            {
                case PlayerState.Idle:
                case PlayerState.Walking:
                case PlayerState.Falling:
                case PlayerState.CustomMovement:
                    _runEffect.Stop();
                    break;

                case PlayerState.Running:
                    _runEffect.Play();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
            }
        }

        private void HandlePlayerJumped()
        {
        }

        #endregion Utils

        #region Structs

        [Serializable]
        private struct EffectSpawnData
        {
            public Vector3 spawnOffset;
            public GameObject effectPrefab;
        }

        #endregion Structs
    }
}