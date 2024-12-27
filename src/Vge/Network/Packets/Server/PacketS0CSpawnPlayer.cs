using Vge.Management;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Пакет спавна сетевого игрока
    /// </summary>
    public struct PacketS0CSpawnPlayer : IPacket
    {
        public byte Id => 0x0C;

        public int Index { get; private set; }
        public string Uuid { get; private set; }
        public string Login { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        //private ItemStack[] stacks;
        //private ArrayList list;

        public PacketS0CSpawnPlayer(PlayerServer entity)
        {
            Uuid = entity.UUID;
            Index = entity.Id;
            Login = entity.Login;
            X = entity.PosX;
            Y = entity.PosY;
            Z = entity.PosZ;
            Yaw = entity.RotationYaw;
            Pitch = entity.RotationPitch;
            //stacks = entity.Inventory.GetCurrentItemAndCloth();
            //list = entity.MetaData.GetAllWatched();
        }

        public void ReadPacket(ReadPacket stream)
        {
            Uuid = stream.String();
            Index = stream.Int();
            Login = stream.String();
            X = stream.Float();
            Y = stream.Float();
            Z = stream.Float();
            Yaw = stream.Float();
            Pitch = stream.Float();
            //int count = stream.ReadByte();
            //stacks = new ItemStack[count];
            //for (int i = 0; i < count; i++)
            //{
            //    stacks[i] = ItemStack.ReadStream(stream);
            //}
            //list = DataWatcher.ReadWatchedListFromPacketBuffer(stream);
        }

        public void WritePacket(WritePacket stream)
        {
            stream.String(Uuid);
            stream.Int(Index);
            stream.String(Login);
            stream.Float(X);
            stream.Float(Y);
            stream.Float(Z);
            stream.Float(Yaw);
            stream.Float(Pitch);
            //int count = stacks.Length;
            //stream.WriteByte((byte)count);
            //for (int i = 0; i < count; i++)
            //{
            //    ItemStack.WriteStream(stacks[i], stream);
            //}
            //DataWatcher.WriteWatchedListToPacketBuffer(list, stream);
        }
    }
}
