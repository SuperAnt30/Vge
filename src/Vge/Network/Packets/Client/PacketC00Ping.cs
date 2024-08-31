namespace Vge.Network.Packets.Client
{
    public struct PacketC00Ping : IPacket
    {
        public byte GetId() => 0x00;

        private long clientTime;
        public long GetClientTime() => clientTime;

        public PacketC00Ping(long time) => clientTime = time;

        public void ReadPacket(ReadPacket stream) => clientTime = stream.Long();
        public void WritePacket(WritePacket stream) => stream.Long(clientTime);

        //ushort us; float x; float y; float z; float v1; float v2; bool b1; byte[] b2;
        //public PacketC00Ping(ushort us, float x, float y, float z, float v1, float v2, bool b1, byte[] b2)
        //{
        //    this.us = us;
        //    this.x = x;
        //    this.y = y;
        //    this.z = z;
        //    this.v1 = v1;
        //    this.v2 = v2;
        //    this.b1 = b1;
        //    this.b2 = b2;
        //}
        
        //public void ReadPacket(ReadPacket stream)
        //{
        //    //clientTime = stream.Long();
        //    ushort us = stream.UShort();
        //    float x = stream.Float();
        //    float y = stream.Float();
        //    float z = stream.Float();
        //    float v1 = stream.Float();
        //    float v2 = stream.Float();
        //    bool b1 = stream.Bool();

        //    byte[] b2 = stream.Bytes();
        //  //  byte[] b2 = stream.BytesDecompress();
        //    //string s3 = stream.String();
        //    return;
        //}

        //public void WritePacket(WritePacket stream)
        //{
        //    stream.UShort(us);
        //    stream.Float(x);
        //    stream.Float(y);
        //    stream.Float(z);
        //    stream.Float(v1);
        //    stream.Float(v2);
        //    stream.Bool(b1);
        //    stream.Bytes(b2);

        //    //stream.Long(clientTime);
        //    //stream.String("Ant");
        //    //stream.String("");

        //    // stream.BytesCompress(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 });
        //    //stream.String("SuperAnt");
        //}


    }
}
