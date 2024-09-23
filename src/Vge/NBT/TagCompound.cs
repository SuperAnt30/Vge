using System.Collections.Generic;

namespace Vge.NBT
{
    /// <summary>
    /// Тэг группы для NBT
    /// </summary>
    public class TagCompound : NBTBase
    {
        /// <summary>
        /// Список тэг
        /// </summary>
        private Dictionary<string, NBTBase> tagMap = new Dictionary<string, NBTBase>();

        public override void Write(NBTStream output)
        {
            foreach (KeyValuePair<string, NBTBase> entry in tagMap)
            {
                NBTBase value = entry.Value;
                output.WriteByte(value.GetId());
                if (value.GetId() != 0)
                {
                    output.WriteUTF(entry.Key);
                    value.Write(output);
                }
            }
            output.WriteByte(0);
        }

        public override void Read(NBTStream input)
        {
            tagMap.Clear();
            byte id;
            string key;
            NBTBase nbt;
            while ((id = input.Byte()) != 0)
            {
                key = input.ReadUTF();
                nbt = CreateNewByType(id);
                nbt.Read(input);
                if (tagMap.ContainsKey(key)) tagMap[key] = nbt;
                else tagMap.Add(key, nbt);
            }
        }

        public override NBTBase Copy()
        {
            TagCompound map = new TagCompound();
            foreach(KeyValuePair<string, NBTBase> entry in tagMap)
            {
                map.SetTag(entry.Key, entry.Value.Copy());
            }
            return map;
        }

        public override byte GetId() => 10;

        /// <summary>
        /// Получает набор с именами ключей в составном теге
        /// </summary>
        public Dictionary<string, NBTBase>.KeyCollection GetKeySet() => tagMap.Keys;

        #region Set

        /// <summary>
        /// Сохраняет данный тег на карте с заданным строковым ключом.
        /// Это в основном используется для хранения списков тегов.
        /// </summary>
        public void SetTag(string key, NBTBase value)
        {
            if (tagMap.ContainsKey(key)) tagMap[key] = value;
            else tagMap.Add(key, value);
        }

        /// <summary>
        /// Сохраняет новый NBTTagByte с заданным строковым ключом
        /// </summary>
        public void SetByte(string key, byte value)
        {
            if (tagMap.ContainsKey(key)) tagMap[key] = new TagByte(value);
            else tagMap.Add(key, new TagByte(value));
        }

        /// <summary>
        /// Сохраняет новый NBTTagShort с заданным строковым ключом
        /// </summary>
        public void SetShort(string key, short value)
        {
            if (tagMap.ContainsKey(key)) tagMap[key] = new TagShort(value);
            else tagMap.Add(key, new TagShort(value));
        }

        /// <summary>
        /// Сохраняет новый NBTTagInt с заданным строковым ключом
        /// </summary>
        public void SetInt(string key, int value)
        {
            if (tagMap.ContainsKey(key)) tagMap[key] = new TagInt(value);
            else tagMap.Add(key, new TagInt(value));
        }

        /// <summary>
        /// Сохраняет новый NBTTagLong с заданным строковым ключом
        /// </summary>
        public void SetLong(string key, long value)
        {
            if (tagMap.ContainsKey(key)) tagMap[key] = new TagLong(value);
            else tagMap.Add(key, new TagLong(value));
        }

        /// <summary>
        /// Сохраняет новый NBTTagFloat с заданным строковым ключом
        /// </summary>
        public void SetFloat(string key, float value)
        {
            if (tagMap.ContainsKey(key)) tagMap[key] = new TagFloat(value);
            else tagMap.Add(key, new TagFloat(value));
        }

        /// <summary>
        /// Сохраняет новый NBTTagString с заданным строковым ключом
        /// </summary>
        public void SetString(string key, string value)
        {
            if (tagMap.ContainsKey(key)) tagMap[key] = new TagString(value);
            else tagMap.Add(key, new TagString(value));
        }

        /// <summary>
        /// Сохраняет новый NBTTagByteArray с заданным строковым ключом
        /// </summary>
        public void SetByteArray(string key, byte[] value)
        {
            if (tagMap.ContainsKey(key)) tagMap[key] = new TagByteArray(value);
            else tagMap.Add(key, new TagByteArray(value));
        }

        /// <summary>
        /// Сохраняет новый NBTTagIntArray с заданным строковым ключом
        /// </summary>
        public void SetIntArray(string key, int[] value)
        {
            if (tagMap.ContainsKey(key)) tagMap[key] = new TagIntArray(value);
            else tagMap.Add(key, new TagIntArray(value));
        }

        /// <summary>
        /// Сохраняет новый NBTTagByte но значение bool с заданным строковым ключом
        /// </summary>
        public void SetBool(string key, bool value)
        {
            if (tagMap.ContainsKey(key)) tagMap[key] = new TagByte((byte)(value ? 1 : 0));
            else tagMap.Add(key, new TagByte((byte)(value ? 1 : 0)));
        }

        #endregion

        /// <summary>
        /// Возвращает, была ли данная строка ранее сохранена в качестве ключа на карте
        /// </summary>
        public bool HasKey(string key) => tagMap.ContainsKey(key);

        /// <summary>
        /// Возвращает, была ли данная строка ранее сохранена в качестве ключа на карте с данным типом
        /// </summary>
        /// <param name="type">99 проверяем на цифровой формат</param>
        public bool HasKey(string key, int type)
        {
            byte typeOld = GetTagType(key);

            if (typeOld == type) return true;
            if (type != 99) return false;
            return typeOld == 1 || typeOld == 2 || typeOld == 3 || typeOld == 4 || typeOld == 5 || typeOld == 6;
        }

        /// <summary>
        /// Вернуть, не имеет ли это соединение тегов
        /// </summary>
        public override bool HasNoTags() => tagMap.Count == 0;

        #region Get

        /// <summary>
        /// Получает общий тег с указанным именем
        /// </summary>
        public NBTBase GetTag(string key) => tagMap.ContainsKey(key) ? tagMap[key] : new TagEnd();

        /// <summary>
        /// Получить Type-ID для записи с заданным ключом
        /// </summary>
        public byte GetTagType(string key) => (byte)(tagMap.ContainsKey(key) ? tagMap[key].GetId() : 0);

        /// <summary>
        /// Извлекает значение байта, используя указанный ключ, или 0, если такой ключ не был сохранен
        /// </summary>
        public byte GetByte(string key)
        {
            if (HasKey(key, 99)) return ((NBTPrimitive)tagMap[key]).GetByte();
            return 0;
        }

        /// <summary>
        /// Извлекает значение short, используя указанный ключ, или 0, если такой ключ не был сохранен
        /// </summary>
        public short GetShort(string key)
        {
            if (HasKey(key, 99)) return ((NBTPrimitive)tagMap[key]).GetShort();
            return 0;
        }

        /// <summary>
        /// Извлекает значение int, используя указанный ключ, или 0, если такой ключ не был сохранен
        /// </summary>
        public int GetInt(string key)
        {
            if (HasKey(key, 99)) return ((NBTPrimitive)tagMap[key]).GetInt();
            return 0;
        }

        /// <summary>
        /// Извлекает значение long, используя указанный ключ, или 0, если такой ключ не был сохранен
        /// </summary>
        public long GetLong(string key)
        {
            if (HasKey(key, 99)) return ((NBTPrimitive)tagMap[key]).GetLong();
            return 0;
        }

        /// <summary>
        /// Извлекает значение float, используя указанный ключ, или 0, если такой ключ не был сохранен
        /// </summary>
        public float GetFloat(string key)
        {
            if (HasKey(key, 99)) return ((NBTPrimitive)tagMap[key]).GetFloat();
            return 0;
        }

        /// <summary>
        /// Извлекает строку, используя указанный ключ, или пусто, если такой ключ не был сохранен
        /// </summary>
        public string GetString(string key)
        {
            if (HasKey(key, 8)) return (tagMap[key]).GetString();
            return "";
        }

        /// <summary>
        /// Извлекает массив байт, используя указанный ключ, или пусто, если такой ключ не был сохранен
        /// </summary>
        public byte[] GetByteArray(string key)
        {
            if (HasKey(key, 7)) return ((TagByteArray)tagMap[key]).GetByteArray();
            return new byte[0];
        }

        /// <summary>
        /// Извлекает массив Int, используя указанный ключ, или пусто, если такой ключ не был сохранен
        /// </summary>
        public int[] GetIntArray(string key)
        {
            if (HasKey(key, 11)) return ((TagIntArray)tagMap[key]).GetIntArray();
            return new int[0];
        }

        /// <summary>
        /// Извлекает NBTTagCompound, используя указанный ключ, или пусто, если такой ключ не был сохранен
        /// </summary>
        public TagCompound GetCompoundTag(string key)
        {
            if (HasKey(key, 10)) return (TagCompound)tagMap[key];
            return new TagCompound();
        }

        /// <summary>
        /// Получает объект NBTtagList с заданным именем. Аргументы: имя, тип NBTBase
        /// </summary>
        public TagList GetTagList(string key, int type)
        {
            if (GetTagType(key) == 9)
            {
                TagList list = tagMap[key] as TagList;
                if (list.TagCount() > 0 && list.GetTagType() == type) return list;
            }
            return new TagList();
        }

        /// <summary>
        /// Извлекает логическое значение, используя указанный ключ, или false, если такой ключ не был сохранен. 
        /// Это использует метод getByte
        /// </summary>
        public bool GetBool(string key) => GetByte(key) != 0;

        #endregion
    }
}
