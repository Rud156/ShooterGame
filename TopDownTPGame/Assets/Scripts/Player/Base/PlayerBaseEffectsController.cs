#region

using System;
using UnityEngine;

#endregion

namespace Player.Base
{
    public class PlayerBaseEffectsController : MonoBehaviour
    {
        [Header("Effects Prefab")]
        [SerializeField] private EffectSpawnData _jumpEffect;
        [SerializeField] private EffectSpawnData _runEffect;
        [SerializeField] private EffectSpawnData _landEffect;

        [Header("Components")]
        [SerializeField] private Transform _player;
        [SerializeField] private BasePlayerController _playerController;

        #region Unity Functions

        private void Start()
        {
            _playerController.OnPlayerGroundedChanged += HandlePlayerGroundedChanged;
            _playerController.OnPlayerStateChanged += HandlePlayerStateChanged;
            _playerController.OnPlayerJumped += HandlePlayerJumped;
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
            // This means we landed on the ground
            if (!previousState && newState)
            {
                var characterTransform = transform;
                var position = characterTransform.position;
                var landEffect = Instantiate(_landEffect.effectPrefab, position, _landEffect.effectPrefab.transform.rotation, characterTransform);
                landEffect.transform.localPosition += _landEffect.spawnOffset;
            }
        }

        private void HandlePlayerStateChanged(PlayerState currentState)
        {
            switch (currentState)
            {
                case PlayerState.Idle:
                case PlayerState.Walking:
                case PlayerState.CustomMovement:
                case PlayerState.CustomInputRestrictingStates:
                    break;

                case PlayerState.Running:
                {
                    var characterTransform = transform;
                    var position = characterTransform.position;
                    var rotation = characterTransform.rotation.eulerAngles;
                    rotation.y += 180;

                    var effect = Instantiate(_runEffect.effectPrefab, position, Quaternion.Euler(rotation), _player);
                    effect.transform.localPosition += _runEffect.spawnOffset;
                }
                    break;

                case PlayerState.Falling:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
            }
        }

        private void HandlePlayerJumped()
        {
            var characterTransform = transform;
            var position = characterTransform.position;
            var landEffect = Instantiate(_jumpEffect.effectPrefab, position, _jumpEffect.effectPrefab.transform.rotation, characterTransform);
            landEffect.transform.localPosition += _jumpEffect.spawnOffset;
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