#region

using System.Collections.Generic;
using CustomCamera;
using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Misc;

#endregion

namespace Player.Type_1
{
    public class WarCryPulse : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _pulseEffectPrefab;
        [SerializeField] private GameObject _pulseBurstEfectPrefab;

        [Header("Pulse Data")]
        [SerializeField] private int _pulseCount;
        [SerializeField] private float _pulseRadius;
        [SerializeField] private float _pulseWaitDuration;
        [SerializeField] private LayerMask _pulseMask;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _cameraShaker;

        [Header("Cooldown Modifier")]
        [SerializeField] private float _cooldownMultiplier;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private Animator _targetAnimator;
        private int _targetAnimTrigger;

        private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];

        private int _currentPulseCount;
        private float _currentPulseWaitDuration;
        private List<Ability> _lastModifiedAbilities;

        private GameObject _lastBurstEffectObject;
        private GameObject _lastPulseEffectObject;

        #region Unity Functions

        private void Start()
        {
            _lastModifiedAbilities = new List<Ability>();
            _currentPulseCount = _pulseCount;
            _currentPulseWaitDuration = 0;
        }

        private void FixedUpdate()
        {
            if (_currentPulseCount <= 0)
            {
                _lastBurstEffectObject.transform.SetParent(null);
                _lastPulseEffectObject.transform.SetParent(null);

                ClearAppliedCooldownPulse();
                Destroy(gameObject);
                return;
            }

            _currentPulseWaitDuration -= Time.fixedDeltaTime;
            if (_currentPulseWaitDuration <= 0)
            {
                ClearAppliedCooldownPulse(); // Reset the Previous Abilities to default
                ApplyCooldownPulse(); // Apply the new Cooldown set

                var abilityTransform = transform;
                var position = abilityTransform.position;
                _lastPulseEffectObject = Instantiate(_pulseEffectPrefab, position, Quaternion.identity, abilityTransform);
                _lastBurstEffectObject = Instantiate(_pulseBurstEfectPrefab, position, Quaternion.identity, abilityTransform);

                _currentPulseCount -= 1;
                _currentPulseWaitDuration = _pulseWaitDuration;

                _targetAnimator.SetTrigger(_targetAnimTrigger);
                CustomCameraController.Instance.StartShake(_cameraShaker);
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void SetTargetAnimatorData(Animator targetAnimator, int targetAnimTrigger)
        {
            _targetAnimator = targetAnimator;
            _targetAnimTrigger = targetAnimTrigger;
        }

        #endregion External Functions

        #region Utils

        private void ClearAppliedCooldownPulse()
        {
            foreach (var ability in _lastModifiedAbilities)
            {
                ability.ResetCooldownMultiplier();
            }
        }

        private void ApplyCooldownPulse()
        {
            var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _pulseRadius, _hitColliders, _pulseMask);
            if (_debugIsActive)
            {
                DebugExtension.DebugWireSphere(transform.position, Color.red, _pulseRadius, _debugDisplayDuration);
            }

            for (var i = 0; i < targetsHit; i++)
            {
                if (_hitColliders[i] == null)
                {
                    continue;
                }

                if (_hitColliders[i].TryGetComponent(out BasePlayerController targetController))
                {
                    var abilities = _hitColliders[i].GetComponents<Ability>();
                    foreach (var ability in abilities)
                    {
                        ability.ChangeCooldownMultiplier(_cooldownMultiplier);
                        _lastModifiedAbilities.Add(ability);
                    }
                }
            }
        }

        #endregion Utils
    }
}