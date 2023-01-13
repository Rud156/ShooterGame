#region

using System;
using UnityEngine;
using UnityEngine.Assertions;

#endregion

namespace Player.Base
{
    public class PlayerBaseEffectsController : MonoBehaviour
    {
        [Header("Effects Prefab")]
        [SerializeField] private EffectSpawnData _fallingEffect;
        [SerializeField] private EffectSpawnData _runEffect;
        [SerializeField] private EffectSpawnData _landEffect;

        [Header("Components")]
        [SerializeField] private Transform _player;
        [SerializeField] private BasePlayerController _playerController;

        private GameObject _fallingEffectObject;

        #region Unity Functions

        private void Start()
        {
            _playerController.OnPlayerGroundedChanged += HandlePlayerGroundedChanged;
            _playerController.OnPlayerStateChanged += HandlePlayerStateChanged;
        }

        private void OnDestroy()
        {
            _playerController.OnPlayerGroundedChanged -= HandlePlayerGroundedChanged;
            _playerController.OnPlayerStatePushed -= HandlePlayerStateChanged;
        }

        #endregion Unity Functions

        #region Utils

        private void HandlePlayerGroundedChanged(bool previousState, bool newState)
        {
            // This means we landed on the ground
            if (!previousState && newState)
            {
                var position = transform.position;
                position += _landEffect.spawnOffset;
                Instantiate(_landEffect.effectPrefab, position, _landEffect.effectPrefab.transform.rotation);
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
                    position += _runEffect.spawnOffset;

                    var rotation = characterTransform.rotation.eulerAngles;
                    Instantiate(_runEffect.effectPrefab, position, Quaternion.Euler(rotation));
                }
                    break;

                case PlayerState.Falling:
                {
                    Assert.IsNull(_fallingEffectObject, "Falling Effect should not have a value here...");
                    var position = transform.position;
                    position += _fallingEffect.spawnOffset;

                    _fallingEffectObject = Instantiate(_fallingEffect.effectPrefab, position, _fallingEffect.effectPrefab.transform.rotation);
                    _fallingEffectObject.transform.SetParent(_player);
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
            }

            if (currentState != PlayerState.Falling)
            {
                if (_fallingEffectObject != null)
                {
                    Destroy(_fallingEffectObject);
                    _fallingEffectObject = null;
                }
            }
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