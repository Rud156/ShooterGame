using Player.Networking.Structs;
using Unity.Netcode;
using World;

namespace Player.Networking
{
    public class PlayerMovementPacketController : NetworkBehaviour
    {
        #region Unity Functions

        private void Start() => WorldTimeManager.Instance.OnWorldCustomFixedUpdate += HandleFixedUpdate;

        public override void OnDestroy()
        {
            base.OnDestroy();
            WorldTimeManager.Instance.OnWorldCustomFixedUpdate -= HandleFixedUpdate;
        }

        #endregion Unity Functions

        #region Sending Data

        private NetworkPacketManager<PlayerSendMovementPacket> _clientToServerPacketManager;
        public NetworkPacketManager<PlayerSendMovementPacket> ClientToServerPacketManager
        {
            get
            {
                _clientToServerPacketManager ??= new NetworkPacketManager<PlayerSendMovementPacket>();
                if (IsLocalPlayer)
                {
                    _clientToServerPacketManager.OnRequireTransmitPackets += TransmitPacketsToServer;
                }

                return _clientToServerPacketManager;
            }
        }

        private void TransmitPacketsToServer(byte[] data) => SendMovementDataToServerRpc(data);

        [ServerRpc]
        private void SendMovementDataToServerRpc(byte[] data) => ClientToServerPacketManager.ReceivePackets(data);

        #endregion Sending Data

        #region Receiving Data

        private NetworkPacketManager<PlayerReceiveMovementPacket> _serverToClientPacketManager;
        public NetworkPacketManager<PlayerReceiveMovementPacket> ServerToClientPacketManager
        {
            get
            {
                _serverToClientPacketManager ??= new NetworkPacketManager<PlayerReceiveMovementPacket>();
                if (IsServer)
                {
                    _serverToClientPacketManager.OnRequireTransmitPackets += TransmitPacketsToClient;
                }
                return _serverToClientPacketManager;
            }
        }

        private void TransmitPacketsToClient(byte[] data) => ReceiveMovementDataToClientRpc(data);

        [ClientRpc] private void ReceiveMovementDataToClientRpc(byte[] data) => ServerToClientPacketManager.ReceivePackets(data);

        #endregion Receiving Data

        #region Misc

        private void HandleFixedUpdate(float fixedUpdateTime)
        {
            ClientToServerPacketManager.FixedUpdate(fixedUpdateTime);
            ServerToClientPacketManager.FixedUpdate(fixedUpdateTime);
        }

        #endregion Misc
    }
}