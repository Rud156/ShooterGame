using Player.Networking.Structs;
using Unity.Netcode;
using World;

namespace Player.Networking
{
    public class PlayerMovementPacketController : NetworkBehaviour
    {
        private NetworkPacketManager<PlayerSendMovementPacket> _clientToServerPacketManager;
        protected NetworkPacketManager<PlayerSendMovementPacket> ClientToServerPacketManager => _clientToServerPacketManager;
        private NetworkPacketManager<PlayerReceiveMovementPacket> _serverToClientPacketManager;
        protected NetworkPacketManager<PlayerReceiveMovementPacket> ServerToClientPacketManager => _serverToClientPacketManager;

        #region Unity Functions

        protected void SetupNetworkObject(float sendRate)
        {
            _clientToServerPacketManager = new NetworkPacketManager<PlayerSendMovementPacket>();
            _serverToClientPacketManager = new NetworkPacketManager<PlayerReceiveMovementPacket>();
            _clientToServerPacketManager.Setup(sendRate);
            _serverToClientPacketManager.Setup(sendRate);

            WorldTimeManager.Instance.OnWorldCustomFixedUpdate += NetworkingFixedUpdate;
            if (IsLocalPlayer)
            {
                _clientToServerPacketManager.OnRequireTransmitPackets += TransmitPacketsToServer;
            }

            if (IsServer)
            {
                _serverToClientPacketManager.OnRequireTransmitPackets += TransmitPacketsToClient;
            }
        }

        protected void DestroyNetworkObject() => WorldTimeManager.Instance.OnWorldCustomFixedUpdate -= NetworkingFixedUpdate;

        private void NetworkingFixedUpdate(float fixedUpdateTime)
        {
            _clientToServerPacketManager.FixedUpdate(fixedUpdateTime);
            _serverToClientPacketManager.FixedUpdate(fixedUpdateTime);
        }

        #endregion Unity Functions

        #region Client to Server Data

        private void TransmitPacketsToServer(byte[] data) => SendMovementDataToServerRpc(data);

        [ServerRpc]
        private void SendMovementDataToServerRpc(byte[] data) => ClientToServerPacketManager.ReceivePackets(data);

        #endregion Client to Server Data

        #region Server to Client Data

        private void TransmitPacketsToClient(byte[] data) => ReceiveMovementDataToClientRpc(data);

        [ClientRpc] private void ReceiveMovementDataToClientRpc(byte[] data) => ServerToClientPacketManager.ReceivePackets(data);

        #endregion Server to Client Data
    }
}