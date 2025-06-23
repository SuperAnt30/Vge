namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Пакет расположения игрока
    /// </summary>
    public struct PacketC04PlayerPosition : IPacket
    {
        public byte Id => 0x04;

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public bool IsSneaking { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool OnGround { get; private set; }

        public bool IsPosition { get; private set; }
        public bool IsRotate { get; private set; }

        public PacketC04PlayerPosition(float x, float y, float z, float yaw, float pitch,
            bool sneaking, bool sprinting, bool onGround)
        {
            X = x;
            Y = y;
            Z = z;
            Yaw = yaw;
            Pitch = pitch;
            OnGround = onGround;
            IsSneaking = sneaking;
            IsSprinting = sprinting;
            IsPosition = true;
            IsRotate = true;
        }
        public PacketC04PlayerPosition(float x, float y, float z, bool sneaking, bool sprinting, bool onGround)
        {
            X = x;
            Y = y;
            Z = z;
            Yaw = Pitch = 0;
            OnGround = onGround;
            IsSneaking = sneaking;
            IsSprinting = sprinting;
            IsPosition = true;
            IsRotate = false;
        }
        public PacketC04PlayerPosition(float yaw, float pitch, bool sneaking, bool sprinting, bool onGround)
        {
            X = Y = Z = 0;
            Yaw = yaw;
            Pitch = pitch;
            OnGround = onGround;
            IsSneaking = sneaking;
            IsSprinting = sprinting;
            IsPosition = false;
            IsRotate = true;
        }
        public PacketC04PlayerPosition(bool sneaking, bool sprinting, bool onGround)
        {
            X = Y = Z = 0;
            Yaw = Pitch = 0;
            OnGround = onGround;
            IsSneaking = sneaking;
            IsSprinting = sprinting;
            IsPosition = false;
            IsRotate = false;
        }

        public void ReadPacket(ReadPacket stream)
        {
            byte flag = stream.Byte();
            IsSprinting = (flag & 1 << 4) != 0;
            IsSneaking = (flag & 1 << 3) != 0;
            IsRotate = (flag & 1 << 2) != 0;
            IsPosition = (flag & 1 << 1) != 0;
            OnGround = (flag & 1) != 0;
            if (IsPosition)
            {
                X = stream.Float();
                Y = stream.Float();
                Z = stream.Float();
            }
            if (IsRotate)
            {
                Yaw = stream.Float();
                Pitch = stream.Float();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            byte flag = 0;
            if (OnGround) flag += 1;
            if (IsPosition) flag += 2;
            if (IsRotate) flag += 4;
            if (IsSneaking) flag += 8;
            if (IsSprinting) flag += 16;
            stream.Byte(flag);
            if (IsPosition)
            {
                stream.Float(X);
                stream.Float(Y);
                stream.Float(Z);
            }
            if (IsRotate)
            {
                stream.Float(Yaw);
                stream.Float(Pitch);
            }
        }
    }
}

