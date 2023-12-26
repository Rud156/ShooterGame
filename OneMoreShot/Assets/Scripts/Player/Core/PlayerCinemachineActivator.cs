using Cinemachine;
using UnityEngine;
using static Utils.Misc.TagManager;

namespace Player.Core
{
    public class PlayerCinemachineActivator : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform _cinemachineFollowTarget;

        #region Unity Functions

        private void Start()
        {
            var cinemachineController = GameObject.FindGameObjectWithTag(PlayerCinemachineController);
            var cinemachineVirtualCamera = cinemachineController.GetComponent<CinemachineVirtualCamera>();
            cinemachineVirtualCamera.Follow = _cinemachineFollowTarget;
            cinemachineVirtualCamera.LookAt = _cinemachineFollowTarget;
        }

        #endregion Unity Functions
    }
}