using UnityEngine;

namespace Ability_Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlasmaBomb : MonoBehaviour
    {
        [Header("Bomb Data")]
        [SerializeField] private float _destroyTime;

        private float _destroyTimeLeft;

        #region Unity Functions

        private void Start() => _destroyTimeLeft = _destroyTime;

        private void FixedUpdate()
        {
            _destroyTimeLeft -= Time.fixedDeltaTime;
            if (_destroyTimeLeft <= 0)
            {
                Destroy(gameObject);
            }
        }

        #endregion Unity Functions
    }
}