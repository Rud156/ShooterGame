#region

using System.Collections.Generic;
using Player.Base;
using UnityEngine;

#endregion

namespace Player.Networking
{
    public struct NetworkPlayerReceiveServerData
    {
        // Core Movement Data
        public Vector3 PlayerPosition;
        public Vector3 PlayerCharacterVelocity;
        public float ModelYRotation;

        // Additional State Data
        public bool JumpPressed;
        public bool IsGrounded;
        public List<PlayerState> PlayerStateStack;
    }
}