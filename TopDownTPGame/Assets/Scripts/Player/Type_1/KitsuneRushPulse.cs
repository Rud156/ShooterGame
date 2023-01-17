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

        private int _currentPulseCount;
        private float _currentPulseWaitDuration;
        private List<Ability> _lastModifiedAbilities;

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
                Instantiate(_pulseEffectPrefab, position, Quaternion.identity, abilityTransform);
                Instantiate(_pulseBurstEfectPrefab, position, Quaternion.identity, abilityTransform);

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
            var hitColliders = new Collider[MaxCollidersCheck];
            var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _pulseRadius, hitColliders, _pulseMask);

            for (var i = 0; i < targetsHit; i++)
            {
                if (hitColliders[i] == null)
                {
                    continue;
                }

                if (hitColliders[i].TryGetComponent(out BasePlayerController targetController))
                {
                    var abilities = hitColliders[i].GetComponents<Ability>();
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