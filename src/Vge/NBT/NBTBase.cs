namespace Vge.NBT
{
    /// <summary>
    /// Абстрактный класс тег (Named Binary Tag — «именованный двоичный тег»)
    /// </summary>
    public abstract class NBTBase
    {
        public abstract byte GetId();
        public abstract void Write(NBTStream output);
        public abstract void Read(NBTStream input);

        protected static NBTBase CreateNewByType(byte id)
        {
            switch (id)
            {
                case 0: return new TagEnd();
                case 1: return new TagByte();
                case 2: return new TagShort();
                case 3: return new TagInt();
                case 4: return new TagLong();
                case 5: case 6: return new TagFloat();
                case 7: return new TagByteArray();
                case 8: return new TagString();
                case 9: return new TagList();
                case 10: return new TagCompound();
                case 11: return new TagIntArray();
                default: return null;
            }
        }

        /// <summary>
        /// Создает клон тега
        /// </summary>
        public abstract NBTBase Copy();

        /// <summary>
        /// Вернуть, не имеет ли это соединение тегов
        /// </summary>
        public virtual bool HasNoTags() => false;

        public override bool Equals(object value)
        {
            if (value is NBTBase)
            {
                NBTBase nBTBase = value as NBTBase;
                return GetId() == nBTBase.GetId();
            }
            return false;
        }

        public override int GetHashCode() => GetId();
        public virtual string GetString() => ToString();
    }

    public abstract class NBTPrimitive : NBTBase
    {
        public abstract long GetLong();
        public abstract int GetInt();
        public abstract short GetShort();
        public abstract byte GetByte();
        public abstract float GetFloat();
    }
}
