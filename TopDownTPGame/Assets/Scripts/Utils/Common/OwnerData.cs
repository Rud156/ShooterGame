using System;
using UnityEngine;

namespace Utils.Common
{
    public class OwnerData : MonoBehaviour
    {
        [Header("Auto Set Owner")]
        [SerializeField] private bool _autoSetOwnerId;
        [SerializeField] private GameObject _parent;

        private int _ownerId;

        #region Unity Functions

        private void Start()
        {
            if (_autoSetOwnerId)
            {
                _ownerId = _parent.GetInstanceID();
            }
        }

        private void OnEnable()
        {
            if (_autoSetOwnerId)
            {
                _ownerId = _parent.GetInstanceID();
            }
        }

        #endregion Unity Functions

        #region External Functions

        public int OwnerId
        {
            get => _ownerId;

            set => _ownerId = value;
        }

        #endregion External Functions
    }
}