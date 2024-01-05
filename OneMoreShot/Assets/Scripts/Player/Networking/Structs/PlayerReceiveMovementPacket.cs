using Player.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Networking.Structs
{
    public class PlayerReceiveMovementPacket : CoreNetworkingPacket
    {
        public Vector3 Position;
        public float YRotation;

        public List<PlayerState> CurrentPlayerState;
        public Vector3 CharacterVelocity;
        public float CurrentStateVelocity;
        public float FrozenMovementDuration;
        public bool IsGrounded;
        public bool JumpReset;
    }
}