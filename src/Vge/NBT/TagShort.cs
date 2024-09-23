namespace Vge.NBT
{
    /// <summary>
    /// Тэг числа 2 байта для NBT
    /// </summary>
    public class TagShort : NBTPrimitive
    {
        private short data = 0;

        public TagShort() { }
        public TagShort(short data) => this.data = data;

        public override void Write(NBTStream output) => output.WriteShort(data);
        public override void Read(NBTStream input) => data = input.ReadShort();

        public override NBTBase Copy() => new TagShort(data);
        public override byte GetId() => 2;

        public override string ToString() => data + "s";

        public override bool Equals(object value)
        {
            if (base.Equals(value))
            {
                TagShort tag = value as TagShort;
                return tag.data == data;
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode() ^ data;

        public override long GetLong() => data;
        public override int GetInt() => data;
        public override short GetShort() => data;
        public override byte GetByte() => (byte)(data & 255);
        public override float GetFloat() => data;
    }
}
