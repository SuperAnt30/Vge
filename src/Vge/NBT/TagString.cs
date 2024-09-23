namespace Vge.NBT
{
    /// <summary>
    /// Тэг строки utf8 для NBT
    /// </summary>
    public class TagString : NBTBase
    {
        private string data = "";

        public TagString() { }
        public TagString(string data)
        {
            this.data = data;
            if (data == null)
            {
                throw new System.Exception(Sr.EmptyStringIsNotAllowed);
            }
        }

        public override void Write(NBTStream output) => output.WriteUTF(data);
        public override void Read(NBTStream input) => data = input.ReadUTF();

        public override NBTBase Copy() => new TagString(data);
        public override byte GetId() => 8;
        /// <summary>
        /// Вернуть, не имеет ли это соединение тегов
        /// </summary>
        //public override bool HasNoTags() => data == "";

        public override string ToString() => "\"" + data.Replace("\"", "\\\"") + "\"";

        public override bool Equals(object value)
        {
            if (base.Equals(value))
            {
                TagString tag = value as TagString;
                return data == null && tag.data == null || data != null && data.Equals(tag.data);
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode() ^ data.GetHashCode();

        public override string GetString() => data;
    }
}
