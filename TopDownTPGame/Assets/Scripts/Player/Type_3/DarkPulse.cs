#region

using Player.Base;
using UnityEngine;

#endregion

namespace Player.Type_3
{
    public class DarkPulse : MonoBehaviour
    {
        private const int MaxCollidersCheck = 10;

        [Header("Pulse Data")]
        [SerializeField] private int _pulseCount;
        [SerializeField] private float _pulseDuration;
        [SerializeField] private float _pulseRadius;
        [SerializeField] private LayerMask _pulseMask;

        private Collider[] _hitColliders = new Collider[MaxCollidersCheck];

        private int _currentPulseCount;
        private float _currentPulseDuration;
        private bool _pulseTriggered;

        #region Unity Functions

        private void FixedUpdate()
        {
            if (!_pulseTriggered)
            {
                var targetsHit = Physics.OverlapSphereNonAlloc(transform.position, _pulseRadius, _hitColliders, _pulseMask);

                for (int i = 0; i < targetsHit; i++)
                {
                    // Do not target itself
                    if (_hitColliders[i] == null || _hitColliders[i].gameObject.GetInstanceID() == gameObject.GetInstanceID())
                    {
                        continue;
                    }

                    // TODO: Also check team here...
                    if (_hitColliders[i].TryGetComponent(out BasePlayerController targetController))
                    {
                        targetController.PlayerEnabledParanoia(_pulseDuration);
                    }
                }

                _pulseTriggered = true;
            }

            _currentPulseDuration -= Time.fixedDeltaTime;
            if (_currentPulseDuration <= 0)
            {
                _currentPulseCount -= 1;
                _currentPulseDuration = _pulseDuration;
                if (_currentPulseCount <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }

        #endregion Unity Functions

        public void StartPulse()
        {
            _currentPulseCount = _pulseCount;
            _currentPulseDuration = _pulseDuration;
            _pulseTriggered = false;
        }
    }
}