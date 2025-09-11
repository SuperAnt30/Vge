using Vge.Entity.MetaData;

namespace Vge.Network.Packets.Server
{
    public struct PacketS1CEntityMetadata : IPacket
    {
        public byte Id => 0x1C;

        public int EntityId { get; private set; }

        public WatchableObject[] Data { get; private set; }

        public PacketS1CEntityMetadata(int entityId, WatchableObject[] data)
        {
            EntityId = entityId;
            Data = data;
        }

        public void ReadPacket(ReadPacket stream)
        {
            EntityId = stream.Int();
            Data = DataWatcher.ReadWatchedListFromPacketBuffer(stream);
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(EntityId);
            DataWatcher.WriteWatchedListToPacketBuffer(Data, stream);
        }
    }
}
