#region

using UnityEngine;

#endregion

namespace Utils.Common
{
    public class DestroyAfterTime : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private float _destroyDelay;

        #region Unity Functions

        private void Start() => Destroy(gameObject, _destroyDelay);

        #endregion Unity Functions
    }
}