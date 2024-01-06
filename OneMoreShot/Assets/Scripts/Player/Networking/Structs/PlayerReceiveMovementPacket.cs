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

        public PlayerReceiveMovementPacket()
        {
            Position = Vector3.zero;
            YRotation = 0;
            CurrentPlayerState = new List<PlayerState>();
            CharacterVelocity = Vector3.zero;
            CurrentStateVelocity = 0;
            FrozenMovementDuration = 0;
            IsGrounded = false;
            JumpReset = false;
        }

        public PlayerReceiveMovementPacket(PlayerReceiveMovementPacket other)
        {
            Position = other.Position;
            YRotation = other.YRotation;
            CurrentPlayerState = new List<PlayerState>(other.CurrentPlayerState);
            CharacterVelocity = other.CharacterVelocity;
            CurrentStateVelocity = other.CurrentStateVelocity;
            FrozenMovementDuration = other.FrozenMovementDuration;
            IsGrounded = other.IsGrounded;
            JumpReset = other.JumpReset;
        }
    }
}