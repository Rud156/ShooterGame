#region

using Cinemachine;
using UnityEngine;
using Utils.Misc;

#endregion

namespace Player.Base
{
    public class SimplePlayerCameraController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform _cinemachineFollowTarget;

        #region Unity Functions

        private void Start()
        {
            var cinemachineController = GameObject.FindGameObjectWithTag(TagManager.PlayerCinemachineController);
            var cinemachineVirtualCamera = cinemachineController.GetComponent<CinemachineVirtualCamera>();
            cinemachineVirtualCamera.Follow = _cinemachineFollowTarget;
            cinemachineVirtualCamera.LookAt = _cinemachineFollowTarget;

            // TODO: Move this to a Util Class/Manager
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        #endregion Unity Functions
    }
}