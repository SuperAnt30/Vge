using Vge.Entity;
using Vge.Entity.MetaData;
using Vge.Item;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Пакет спавна моба
    /// </summary>
    public struct PacketS0FSpawnMob : IPacket
    {
        public byte Id => 0x0F;

        public int Index { get; private set; }
        public ushort IndexEntity { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public bool OnGround { get; private set; }
        public WatchableObject[] Data { get; private set; }
        public ItemStack[] Stacks { get; private set; }

        private bool _isLiving;

        public PacketS0FSpawnMob(EntityBase entity)
        {
            Index = entity.Id;
            IndexEntity = entity.IndexEntity;
            X = entity.PosX;
            Y = entity.PosY;
            Z = entity.PosZ;
            OnGround = entity.OnGround;

            if (entity is EntityLiving entityLiving)
            {
                _isLiving = true;
                Yaw = entityLiving.RotationYaw;
                Pitch = entityLiving.RotationPitch;
                if (entityLiving.Inventory != null)
                {
                    Stacks = entityLiving.Inventory.GetCurrentItemAndCloth();
                }
                else
                {
                    Stacks = new ItemStack[0];
                }
            }
            else
            {
                _isLiving = false;
                Yaw = Pitch = 0;
                Stacks = null;
            }

            Data = entity.MetaData.GetAllWatched();
        }

        public void ReadPacket(ReadPacket stream)
        {
            Index = stream.Int();
            IndexEntity = stream.UShort();
            X = stream.Float();
            Y = stream.Float();
            Z = stream.Float();
            OnGround = stream.Bool();
            _isLiving = stream.Bool();
            if (_isLiving)
            {
                Yaw = stream.Float();
                Pitch = stream.Float();
                int count = stream.Byte();
                Stacks = new ItemStack[count];
                for (int i = 0; i < count; i++)
                {
                    Stacks[i] = ItemStack.ReadStream(stream);
                }
            }
            Data = DataWatcher.ReadWatchedListFromPacketBuffer(stream);
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(Index);
            stream.UShort(IndexEntity);
            stream.Float(X);
            stream.Float(Y);
            stream.Float(Z);
            stream.Bool(OnGround);
            stream.Bool(_isLiving);
            if (_isLiving)
            {
                stream.Float(Yaw);
                stream.Float(Pitch);
                int count = Stacks.Length;
                stream.Byte((byte)count);
                for (int i = 0; i < count; i++)
                {
                    ItemStack.WriteStream(Stacks[i], stream);
                }
            }
            DataWatcher.WriteWatchedListToPacketBuffer(Data, stream);
        }
    }
}
