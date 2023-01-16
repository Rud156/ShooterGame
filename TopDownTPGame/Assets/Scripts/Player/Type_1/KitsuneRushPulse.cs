#region

using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_1
{
    public class KitsuneRushPulse : MonoBehaviour
    {
        private const int MaxCollidersCheck = 10;

        [Header("Pulse Data")]
        [SerializeField] private float _pulseCount;
        [SerializeField] private float _pulseRadius;
        [SerializeField] private float _pulseWaitDuration;
        [SerializeField] private LayerMask _pulseMask;

        [Header("Cooldown Modifier")]
        [SerializeField] private float _cooldownMultiplier;

        #region Unity Functions

        private void Update()
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
                        // TODO: Complete this function...
                    }
                }
            }
        }

        #endregion Unity Functions
    }
}