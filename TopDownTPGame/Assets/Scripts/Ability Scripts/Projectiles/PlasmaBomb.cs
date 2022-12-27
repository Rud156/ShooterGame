using UnityEngine;

namespace AbilityScripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlasmaBomb : MonoBehaviour
    {
        [Header("Bomb Data")]
        [SerializeField] private float _destroyTime;

        private float _timeLeft;

        #region Unity Functions

        private void Start() => _timeLeft = _destroyTime;

        private void FixedUpdate()
        {
            _timeLeft -= Time.fixedDeltaTime;
            if (_timeLeft <= 0)
            {
                Destroy(gameObject);
            }
        }

        #endregion Unity Functions
    }
}