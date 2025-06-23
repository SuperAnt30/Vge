using Vge.Entity.Player;

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
        public byte IdWorld { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public bool OnGround { get; private set; }
        //private ItemStack[] stacks;
        //private ArrayList list;

        public PacketS0CSpawnPlayer(PlayerServer player)
        {
            Uuid = player.UUID;
            Index = player.Id;
            Login = player.Login;
            IdWorld = player.IdWorld;
            X = player.PosX;
            Y = player.PosY;
            Z = player.PosZ;
            Yaw = player.RotationYaw;
            Pitch = player.RotationPitch;
            OnGround = player.OnGround;
            //stacks = entity.Inventory.GetCurrentItemAndCloth();
            //list = entity.MetaData.GetAllWatched();
        }

        public void ReadPacket(ReadPacket stream)
        {
            Uuid = stream.String();
            Index = stream.Int();
            Login = stream.String();
            IdWorld = stream.Byte();
            X = stream.Float();
            Y = stream.Float();
            Z = stream.Float();
            Yaw = stream.Float();
            Pitch = stream.Float();
            OnGround = stream.Bool();
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
            stream.Byte(IdWorld);
            stream.Float(X);
            stream.Float(Y);
            stream.Float(Z);
            stream.Float(Yaw);
            stream.Float(Pitch);
            stream.Bool(OnGround);
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
