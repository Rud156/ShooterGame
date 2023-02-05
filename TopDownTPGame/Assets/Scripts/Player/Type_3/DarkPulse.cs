#region

using HealthSystem;
using Player.Base;
using UnityEngine;
using Utils.Misc;

#endregion

namespace Player.Type_3
{
    public class DarkPulse : MonoBehaviour
    {
        private const int MaxCollidersCheck = 10;

        [Header("Prefabs")]
        [SerializeField] private GameObject _paranoiaPrefab;
        [SerializeField] private GameObject _pulseEffectPrefab;
        [SerializeField] private GameObject _pulseBurstEffectPrefab;

        [Header("Pulse Data")]
        [SerializeField] private int _pulseCount;
        [SerializeField] private float _pulseDuration;
        [SerializeField] private float _pulseRadius;
        [SerializeField] private LayerMask _pulseMask;

        [Header("Affect Data")]
        [SerializeField] private int _healthDecayAmount;
        [SerializeField] private float _healthDecayDuration;

        private Collider[] _hitColliders = new Collider[MaxCollidersCheck];

        private int _currentPulseCount;
        private float _currentPulseWaitDuration;

        private GameObject _lastBurstEffectObject;
        private GameObject _lastPulseEffectObject;

        private int _ownerId;

        #region Unity Functions

        private void Start()
        {
            _currentPulseCount = _pulseCount;
            _currentPulseWaitDuration = 0;
        }

        private void FixedUpdate()
        {
            if (_currentPulseCount <= 0)
            {
                _lastBurstEffectObject.transform.SetParent(null);
                _lastPulseEffectObject.transform.SetParent(null);

                Destroy(gameObject);
                return;
            }

            _currentPulseWaitDuration -= Time.fixedDeltaTime;
            if (_currentPulseWaitDuration <= 0)
            {
                _currentPulseCount -= 1;
                _currentPulseWaitDuration = _pulseDuration;
                ApplyParanoiaPulse();

                var abilityTransform = transform;
                var position = abilityTransform.position;
                _lastPulseEffectObject = Instantiate(_pulseEffectPrefab, position, Quaternion.identity, abilityTransform);
                _lastBurstEffectObject = Instantiate(_pulseBurstEffectPrefab, position, Quaternion.identity, abilityTransform);
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void SetOwnerInstanceId(int ownerId) => _ownerId = ownerId;

        #endregion External Functions

        #region Utils

        private void ApplyParanoiaPulse()
        {
            var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _pulseRadius, _hitColliders, _pulseMask);
            for (var i = 0; i < targetsHit; i++)
            {
                // Do not target itself
                if (_hitColliders[i] == null)
                {
                    continue;
                }

                if (_hitColliders[i].TryGetComponent(out BasePlayerController targetController))
                {
                    var position = targetController.transform.position;
                    var paranoia = Instantiate(_paranoiaPrefab, position, Quaternion.identity);

                    var targetTransform = targetController.transform;
                    var cameraTransform = targetTransform.Find(TagManager.Camera);
                    paranoia.transform.SetParent(cameraTransform);
                    paranoia.transform.localPosition = Vector3.zero;

                    var paranoiaEffect = paranoia.GetComponent<Type_3_Ultimate_DarkPulseParanoia>();
                    targetController.CheckAndAddExternalAbility(paranoiaEffect);
                }

                if (_hitColliders[i].TryGetComponent(out HealthAndDamage healthAndDamage))
                {
                    healthAndDamage.ApplyHealthDecay(_healthDecayAmount, _healthDecayDuration);
                }
            }
        }

        #endregion Utils
    }
}