namespace Vge.NBT
{
    /// <summary>
    /// Тэг байта для NBT
    /// </summary>
    public class TagByte : NBTPrimitive
    {
        private byte data = 0;

        public TagByte() { }
        public TagByte(byte data) => this.data = data;

        public override void Write(NBTStream output) => output.WriteByte(data);
        public override void Read(NBTStream input) => data = input.Byte();

        public override NBTBase Copy() => new TagByte(data);
        public override byte GetId() => 1;

        public override string ToString() => data + "b";

        public override bool Equals(object value)
        {
            if (base.Equals(value))
            {
                TagByte tag = value as TagByte;
                return tag.data == data;
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode() ^ data;

        public override long GetLong() => data;
        public override int GetInt() => data;
        public override short GetShort() => data;
        public override byte GetByte() => data;
        public override float GetFloat() => data;
    }
}
