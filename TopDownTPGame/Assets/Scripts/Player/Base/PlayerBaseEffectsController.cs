#region

using System;
using UnityEngine;
using Utils.Misc;

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

            var runEffectObject = Instantiate(_runEffectPrefab.effectPrefab, transform.position, Quaternion.Euler(_runEffectPrefab.spawnRotation));
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

        private void Update()
        {
            var playerState = _playerController.GetTopPlayerState();
            switch (playerState)
            {
                case PlayerState.Idle:
                case PlayerState.Walking:
                case PlayerState.Falling:
                case PlayerState.CustomMovement:
                    break;

                case PlayerState.Running:
                {
                    var startStateVelocity = _playerController.GetStartStateVelocity();
                    var targetStateVelocity = _playerController.GetTargetStateVelocity();
                    var currentStateVelocity = _playerController.GetCurrentStateVelocity();

                    var mappedEmissionRate = ExtensionFunctions.Map(
                        currentStateVelocity,
                        startStateVelocity,
                        targetStateVelocity,
                        _runEffectPrefab.minEmissionRate,
                        _runEffectPrefab.maxEmissionRate
                    );
                    if (ExtensionFunctions.IsNearlyEqual(startStateVelocity, targetStateVelocity))
                    {
                        mappedEmissionRate = _runEffectPrefab.maxEmissionRate;
                    }

                    var emissionSystem = _runEffect.emission;
                    emissionSystem.rateOverTime = mappedEmissionRate;
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion Unity Functions

        #region Utils

        private void HandlePlayerGroundedChanged(bool previousState, bool newState)
        {
            if (newState)
            {
                var landEffectObject = Instantiate(_landEffectPrefab.effectPrefab, transform.position, Quaternion.Euler(_landEffectPrefab.spawnRotation));
                landEffectObject.transform.SetParent(transform);
                landEffectObject.transform.localPosition += _landEffectPrefab.spawnOffset;
                landEffectObject.transform.SetParent(null);
            }
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
            if (!_playerController.IsGrounded)
            {
                return;
            }

            var jumpEffectObject = Instantiate(_jumpEffectPrefab.effectPrefab, transform.position, Quaternion.Euler(_jumpEffectPrefab.spawnRotation));
            jumpEffectObject.transform.SetParent(transform);
            jumpEffectObject.transform.localPosition += _jumpEffectPrefab.spawnOffset;
            jumpEffectObject.transform.SetParent(null);
        }

        #endregion Utils

        #region Structs

        [Serializable]
        private struct EffectSpawnData
        {
            public Vector3 spawnOffset;
            public Vector3 spawnRotation;
            public GameObject effectPrefab;
            public float minEmissionRate;
            public float maxEmissionRate;
        }

        #endregion Structs
    }
}