#region

using UnityEngine;

#endregion

namespace Ability_Scripts.Spawns
{
    public class Type_5_TwoWayShieldController : MonoBehaviour
    {
        [Header("Shield Data")]
        [SerializeField] private Transform _parent;
        [SerializeField] private float _shieldDestroyDuration;

        private float _destroyTimeLeft;

        #region Unity Functions

        private void Start() => _destroyTimeLeft = _shieldDestroyDuration;

        private void OnTriggerEnter(Collider other) => Destroy(other.gameObject);

        private void Update()
        {
            _destroyTimeLeft -= Time.deltaTime;
            if (_destroyTimeLeft <= 0)
            {
                Destroy(_parent.gameObject);
            }
        }

        #endregion Unity Functions
    }
}