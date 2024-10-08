﻿using System.Numerics;

namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Пакет расположения игрока
    /// </summary>
    public struct PacketC04PlayerPosition : IPacket
    {
        public byte GetId() => 0x04;

        private Vector3 pos;
        private bool sneaking;
        private bool sprinting;
        private bool onGround;

        public Vector3 GetPos() => pos;
        public bool IsSneaking() => sneaking;
        public bool IsSprinting() => sprinting;
        public bool OnGround() => onGround;

        public PacketC04PlayerPosition(Vector3 pos, bool sneaking, bool sprinting, bool onGround)
        {
            this.pos = pos;
            this.sneaking = sneaking;
            this.sprinting = sprinting;
            this.onGround = onGround;
        }

        public void ReadPacket(ReadPacket stream)
        {
            pos = new Vector3(stream.Float(), stream.Float(), stream.Float());
            byte b = stream.Byte();
            //sneaking = stream.Bool();
            //sprinting = stream.Bool();
            //onGround = stream.Bool();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Float(pos.X);
            stream.Float(pos.Y);
            stream.Float(pos.Z);
            stream.Byte(0);
            //stream.Bool(sneaking);
            //stream.Bool(sprinting);
            //stream.Bool(onGround);
        }
    }
}
