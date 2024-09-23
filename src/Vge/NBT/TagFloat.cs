using WinGL.Util;

namespace Vge.NBT
{
    /// <summary>
    /// Тэг чесла с плавающей запятой для NBT
    /// </summary>
    public class TagFloat : NBTPrimitive
    {
        private float data = 0;

        public TagFloat() { }
        public TagFloat(float data) => this.data = data;

        public override void Write(NBTStream output) => output.WriteFloat(data);
        public override void Read(NBTStream input) => data = input.ReadFloat();

        public override NBTBase Copy() => new TagFloat(data);
        public override byte GetId() => 5;

        public override string ToString() => data + "f";

        public override bool Equals(object value)
        {
            if (base.Equals(value))
            {
                TagFloat tag = value as TagFloat;
                return tag.data == data;
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode() ^ data.GetHashCode();

        public override long GetLong() => (long)data;
        public override int GetInt() => Mth.Floor(data);
        public override short GetShort() => (short)(Mth.Floor(data) & 65535);
        public override byte GetByte() => (byte)(Mth.Floor(data) & 255);
        public override float GetFloat() => data;
    }
}
