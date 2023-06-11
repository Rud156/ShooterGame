#region

using CustomCamera;
using HealthSystem;
using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Common;

#endregion

namespace Player.Type_3
{
    public class Type_3_Ultimate_DarkPulseParanoia : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _paranoiaPrefab;
        [SerializeField] private GameObject _pulseEffectPrefab;
        [SerializeField] private GameObject _pulseBurstEffectPrefab;

        [Header("Pulse Data")]
        [SerializeField] private int _pulseCount;
        [SerializeField] private float _pulseDuration;
        [SerializeField] private float _pulseRadius;
        [SerializeField] private LayerMask _pulseMask;

        [Header("Camera Data")]
        [SerializeField] private CameraShaker _cameraShaker;

        [Header("Affect Data")]
        [SerializeField] private int _healthDecayAmount;
        [SerializeField] private float _healthDecayDuration;

        private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];

        private OwnerData _ownerIdData;

        private int _currentPulseCount;
        private float _currentPulseWaitDuration;

        private GameObject _lastBurstEffectObject;
        private GameObject _lastPulseEffectObject;

        private Animator _targetAnimator;
        private int _targetAnimTrigger;

        #region Unity Functions

        private void Start()
        {
            _currentPulseCount = _pulseCount;
            _currentPulseWaitDuration = 0;
            _ownerIdData = GetComponent<OwnerData>();
        }

        private void Update()
        {
            if (_currentPulseCount <= 0)
            {
                _lastBurstEffectObject.transform.SetParent(null);
                _lastPulseEffectObject.transform.SetParent(null);

                Destroy(gameObject);
                return;
            }

            _currentPulseWaitDuration -= Time.deltaTime;
            if (_currentPulseWaitDuration <= 0)
            {
                _currentPulseCount -= 1;
                _currentPulseWaitDuration = _pulseDuration;
                _targetAnimator.SetTrigger(_targetAnimTrigger);

                var abilityTransform = transform;
                var position = abilityTransform.position;
                _lastPulseEffectObject = Instantiate(_pulseEffectPrefab, position, Quaternion.identity, abilityTransform);
                _lastBurstEffectObject = Instantiate(_pulseBurstEffectPrefab, position, Quaternion.identity, abilityTransform);

                ApplyParanoiaPulse();
                CustomCameraController.Instance.StartShake(_cameraShaker);
            }
        }

        #endregion Unity Functions

        #region External Functions

        public void SetOwnerInstanceId(Animator targetAnimator, int targetAnimTrigger)
        {
            _targetAnimator = targetAnimator;
            _targetAnimTrigger = targetAnimTrigger;
        }

        #endregion External Functions

        #region Utils

        private void ApplyParanoiaPulse()
        {
            var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _pulseRadius, _hitColliders, _pulseMask);
            for (var i = 0; i < targetsHit; i++)
            {
                var hitOwnerData = _hitColliders[i].GetComponent<OwnerData>();
                if (hitOwnerData == null || hitOwnerData.OwnerId == _ownerIdData.OwnerId)
                {
                    continue;
                }

                if (_hitColliders[i].TryGetComponent(out BasePlayerController targetController))
                {
                    var position = targetController.transform.position;
                    var paranoia = Instantiate(_paranoiaPrefab, position, Quaternion.identity);

                    var paranoiaEffect = paranoia.GetComponent<Type_3_Ultimate_DarkPulseParanoiaEffect>();
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