using Vge.Entity.MetaData;

namespace Vge.Network.Packets.Server
{
    public struct PacketS1CEntityMetadata : IPacket
    {
        public byte Id => 0x1C;

        public int EntityId { get; private set; }

        public WatchableObject[] List { get; private set; }

        public PacketS1CEntityMetadata(int entityId, WatchableObject[] list)
        {
            EntityId = entityId;
            List = list;
        }

        public void ReadPacket(ReadPacket stream)
        {
            EntityId = stream.Int();
            List = DataWatcher.ReadWatchedListFromPacketBuffer(stream);
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(EntityId);
            DataWatcher.WriteWatchedListToPacketBuffer(List, stream);
        }
    }
}
