using Player.Base;
using UnityEngine;

namespace AbilityScripts.Projectiles
{
    public class DarkPulse : MonoBehaviour
    {
        private const int MAX_COLLIDERS_CHECK = 10;

        [Header("Pulse Data")]
        [SerializeField] private int _pulseCount;
        [SerializeField] private float _pulseDuration;
        [SerializeField] private float _pulseRadius;
        [SerializeField] private LayerMask _pulseMask;

        private int _currentCount;
        private float _currentDuration;
        private bool _pulseTriggered;

        #region Unity Functions

        private void FixedUpdate()
        {
            if (!_pulseTriggered)
            {
                Collider[] hitColliders = new Collider[MAX_COLLIDERS_CHECK];
                Physics.OverlapSphereNonAlloc(transform.position, _pulseRadius, hitColliders, _pulseMask);

                for (int i = 0; i < MAX_COLLIDERS_CHECK; i++)
                {
                    // Do not target itself
                    if (hitColliders[i] == null || hitColliders[i].gameObject.GetInstanceID() == gameObject.GetInstanceID())
                    {
                        continue;
                    }

                    // TODO: Also check team here...
                    if (hitColliders[i].TryGetComponent(out BasePlayerController targetController))
                    {
                        targetController.PlayerEnabledParanoia(_pulseDuration);
                    }
                }

                _pulseTriggered = true;
            }

            _currentDuration -= Time.fixedDeltaTime;
            if (_currentDuration <= 0)
            {
                _currentCount -= 1;
                _currentDuration = _pulseDuration;
                if (_currentCount <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }

        #endregion Unity Functions

        public void StartPulse()
        {
            _currentCount = _pulseCount;
            _currentDuration = _pulseDuration;
            _pulseTriggered = false;
        }
    }
}