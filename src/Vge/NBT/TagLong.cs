namespace Vge.NBT
{
    /// <summary>
    /// Тэг числа 8 байт для NBT
    /// </summary>
    public class TagLong : NBTPrimitive
    {
        private long data = 0;

        public TagLong() { }
        public TagLong(long data) => this.data = data;

        public override void Write(NBTStream output) => output.WriteLong(data);
        public override void Read(NBTStream input) => data = input.ReadLong();

        public override NBTBase Copy() => new TagLong(data);
        public override byte GetId() => 4;

        public override string ToString() => data + "L";

        public override bool Equals(object value)
        {
            if (base.Equals(value))
            {
                TagLong tag = value as TagLong;
                return tag.data == data;
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode() ^ data.GetHashCode();

        public override long GetLong() => data;
        public override int GetInt() => (int)(data & -1L);
        public override short GetShort() => (short)(data & 65535L);
        public override byte GetByte() => (byte)(data & 255L);
        public override float GetFloat() => data;
    }
}
