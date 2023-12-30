#region

using System;
using UnityEngine;

#endregion

namespace Utils.Common
{
    public class OwnerData : MonoBehaviour
    {
        [Header("Set Owner Data From Parent")]
        [SerializeField] private bool _autoSetOwnerIdFromParent;
        [SerializeField] private GameObject _parent;

        [Header("Self Data")]
        [SerializeField] private bool _isSelfOwner;

        private string _ownerId;

        #region Unity Functions

        private void Awake()
        {
            if (_isSelfOwner)
            {
                _ownerId = Guid.NewGuid().ToString();
            }
        }

        private void Start()
        {
            if (_autoSetOwnerIdFromParent)
            {
                _ownerId = _parent.GetComponent<OwnerData>().OwnerId;
            }
        }

        private void OnEnable()
        {
            if (_autoSetOwnerIdFromParent)
            {
                _ownerId = _parent.GetComponent<OwnerData>().OwnerId;
            }
        }

        #endregion Unity Functions

        #region External Functions

        public string OwnerId
        {
            get => _ownerId;

            set => _ownerId = value;
        }

        #endregion External Functions
    }
}