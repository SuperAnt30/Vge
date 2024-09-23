namespace Vge.NBT
{
    /// <summary>
    /// Тэг числа 4 байта для NBT
    /// </summary>
    public class TagInt : NBTPrimitive
    {
        private int data = 0;

        public TagInt() { }
        public TagInt(int data) => this.data = data;

        public override void Write(NBTStream output) => output.WriteInt(data);
        public override void Read(NBTStream input) => data = input.ReadInt();

        public override NBTBase Copy() => new TagInt(data);
        public override byte GetId() => 3;

        public override string ToString() => data + "i";

        public override bool Equals(object value)
        {
            if (base.Equals(value))
            {
                TagInt tag = value as TagInt;
                return tag.data == data;
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode() ^ data;

        public override long GetLong() => data;
        public override int GetInt() => data;
        public override short GetShort() => (short)(data & 65535);
        public override byte GetByte() => (byte)(data & 255);
        public override float GetFloat() => data;
    }
}
