#region

using System.Collections.Generic;
using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_1
{
    public class KitsuneRushPulse : MonoBehaviour
    {
        private const int MaxCollidersCheck = 10;

        [Header("Prefabs")]
        [SerializeField] private GameObject _pulseEffectPrefab;
        [SerializeField] private GameObject _pulseBurstEfectPrefab;

        [Header("Pulse Data")]
        [SerializeField] private int _pulseCount;
        [SerializeField] private float _pulseRadius;
        [SerializeField] private float _pulseWaitDuration;
        [SerializeField] private LayerMask _pulseMask;

        [Header("Cooldown Modifier")]
        [SerializeField] private float _cooldownMultiplier;

        private Collider[] _hitColliders = new Collider[MaxCollidersCheck];

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
            }
        }

        #endregion Unity Functions

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