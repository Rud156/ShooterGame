#region

using Player.Common;
using UnityEngine;

#endregion

namespace Ability_Scripts.Spawns
{
    public class Type_5_TwoWayShieldController : MonoBehaviour
    {
        [Header("Shield Data")]
        [SerializeField] private float _shieldDestroyDuration;

        [Header("Collision Detection")]
        [SerializeField] private float _outerSphereRadius;
        [SerializeField] private float _innerSphereRadius;
        [SerializeField] private LayerMask _sphereCollisionMask;


        private Collider[] _outerSphereColliders = new Collider[PlayerStaticData.MaxCollidersCheck];
        private Collider[] _innerSphereColliders = new Collider[PlayerStaticData.MaxCollidersCheck];
        private float _destroyTimeLeft;

        #region Unity Functions

        private void Start() => _destroyTimeLeft = _shieldDestroyDuration;

        private void Update()
        {
            UpdateCollisionCheck();
            UpdateDestroyTimer();
        }

        #endregion Unity Functions

        #region Utils

        private void UpdateCollisionCheck()
        {
            var position = transform.position;
            var outerSphereHitCount = Physics.OverlapSphereNonAlloc(position, _outerSphereRadius, _outerSphereColliders, _sphereCollisionMask);
            var innerSphereHitCount = Physics.OverlapSphereNonAlloc(position, _innerSphereRadius, _innerSphereColliders, _sphereCollisionMask);

            for (var i = 0; i < outerSphereHitCount; i++)
            {
                if (_outerSphereColliders[i] == null)
                {
                    continue;
                }

                var outerGameObjectId = _outerSphereColliders[i].gameObject.GetInstanceID();
                var collidedWithInner = false;
                for (var j = 0; j < innerSphereHitCount; j++)
                {
                    if (_innerSphereColliders[i] == null)
                    {
                        continue;
                    }

                    var innerGameObjectId = _innerSphereColliders[i].gameObject.GetInstanceID();
                    if (innerGameObjectId == outerGameObjectId)
                    {
                        collidedWithInner = true;
                        break;
                    }
                }

                if (!collidedWithInner)
                {
                    Destroy(_outerSphereColliders[i].gameObject);
                }
            }
        }

        private void UpdateDestroyTimer()
        {
            _destroyTimeLeft -= Time.deltaTime;
            if (_destroyTimeLeft <= 0)
            {
                Destroy(gameObject);
            }
        }

        #endregion Utils
    }
}