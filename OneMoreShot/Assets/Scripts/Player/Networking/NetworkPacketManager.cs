using OdinSerializer;
using Player.Networking.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Player.Networking
{
    public class NetworkPacketManager<T> where T : CoreNetworkingPacket
    {
        private const int MaxPacketsBeforeSending = 10;
        private float _sendRate;
        private float _lastSendTime;

        private List<T> _packetsToSend = new();
        private List<T> _receivedPackets = new();
        public int ReceivedPacketsLength => _receivedPackets.Count;

        public delegate void RequireTransmitPackets(byte[] data);
        public RequireTransmitPackets OnRequireTransmitPackets;

        #region Packet Data Utils

        public void Setup(float sendRate)
        {
            _packetsToSend = new List<T>();
            _receivedPackets = new List<T>();
            _sendRate = sendRate;
        }

        public void AddPacket(T packet)
        {
            _packetsToSend.Add(packet);
            if (_packetsToSend.Count >= MaxPacketsBeforeSending)
            {
                SendPackets();
            }
        }

        public void SetAllPackets(List<T> packetsToSend)
        {
            _packetsToSend.Clear();
            _packetsToSend.AddRange(packetsToSend);
            if (_packetsToSend.Count >= MaxPacketsBeforeSending)
            {
                SendPackets();
            }
        }

        public void ReceivePackets(byte[] data)
        {
            List<T> receivedPackets = ReadBytes(data);
            _receivedPackets.AddRange(receivedPackets);
            _receivedPackets = _receivedPackets.Distinct(new PacketEqualityComparer()).ToList();
            _receivedPackets.Sort((x, y) => x.TimeStamp.CompareTo(y.TimeStamp));
        }

        public T GetNextReceivedData()
        {
            if (_receivedPackets == null || _receivedPackets.Count <= 0)
            {
                return default;
            }

            T packet = _receivedPackets[0];
            _receivedPackets.RemoveAt(0);
            return packet;
        }

        public void ClearReceivedQueue() => _receivedPackets?.Clear();

        public void ClearReceivedQueueOfOldData(float timeStamp) => _receivedPackets?.RemoveAll(_ => _.TimeStamp <= timeStamp);

        #endregion Packet Data Utils

        #region Packet Data Controller

        private byte[] CreateBytes(List<T> packets) => SerializationUtility.SerializeValue(packets, DataFormat.Binary);

        private List<T> ReadBytes(byte[] data) => SerializationUtility.DeserializeValue<List<T>>(data, DataFormat.Binary);

        public void FixedUpdate(float fixedDeltaTime)
        {
            _lastSendTime += fixedDeltaTime;
            if (_lastSendTime >= _sendRate && _packetsToSend.Count > 0)
            {
                _lastSendTime = 0;
                SendPackets();
            }
        }

        private void SendPackets()
        {
            byte[] data = CreateBytes(_packetsToSend);
            _packetsToSend.Clear();
            OnRequireTransmitPackets?.Invoke(data);
        }

        #endregion Packet Data Controller

        #region Comparer

        private class PacketEqualityComparer : IEqualityComparer<T>
        {
            public bool Equals(T x, T y) => x.TimeStamp == y.TimeStamp;

            public int GetHashCode(T obj) => obj.TimeStamp.GetHashCode();
        }

        #endregion Comparer
    }
}