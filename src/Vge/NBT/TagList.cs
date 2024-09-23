using System;
using System.Collections.Generic;

namespace Vge.NBT
{
    /// <summary>
    /// Тэг листа для NBT
    /// </summary>
    public class TagList : NBTBase
    {
        private List<NBTBase> tagList = new List<NBTBase>();
        private byte tagType = 0;

        public TagList() { }
        public TagList(params float[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                AppendTag(new TagFloat(objects[i]));
            }
        }

        public override void Write(NBTStream output)
        {
            tagType = tagList.Count == 0 ? (byte)0 : tagList[0].GetId();

            output.WriteByte(tagType);
            output.WriteInt(tagList.Count);

            for (int i = 0; i < tagList.Count; i++)
            {
                tagList[i].Write(output);
            }
        }
        public override void Read(NBTStream input)
        {
            tagType = input.Byte();
            int count = input.ReadInt();
            tagList.Clear();
            NBTBase nbt;
            for (int i = 0; i < count; i++)
            {
                nbt = CreateNewByType(tagType);
                nbt.Read(input);
                tagList.Add(nbt);
            }
        }

        public override NBTBase Copy()
        {
            TagList list = new TagList { tagType = tagType };
            for (int i = 0; i < tagList.Count; i++)
            {
                list.tagList.Add(tagList[i].Copy());
            }
            return list;
        }
        public override byte GetId() => 9;

        public override string ToString()
        {
            string str = "[";
            for (int i = 0; i < tagList.Count; i++)
                str += "" + i + ":" + tagList[i] + ",";
            str += "]";
            return str;
        }

        public override bool Equals(object value)
        {
            if (base.Equals(value))
            {
                TagList list = value as TagList;
                if (tagType == list.tagType)
                {
                    return tagList.Equals(list.tagList);
                }
            }
            return false;
        }
            
        public override int GetHashCode() => base.GetHashCode() ^ tagList.GetHashCode();

        /// <summary>
        /// Вернуть, не имеет ли это соединение тегов
        /// </summary>
        public override bool HasNoTags() => tagList.Count == 0;

        public int GetTagType() => tagType;

        public int TagCount() => tagList.Count;

        public NBTBase Get(int index) => index >= 0 && index < tagList.Count ? tagList[index] : new TagEnd();

        public void Set(int index, NBTBase value)
        {
            if (index >= 0 && index < tagList.Count)
            {
                if (tagType == 0)
                {
                    tagType = value.GetId();
                }
                else if (tagType != value.GetId())
                {
                    throw new Exception(Sr.AddingInappropriateTagTypesToList);
                }
                tagList[index] = value;
            }
            else
            {
                throw new Exception(Sr.IndexOutOfBoundsToSetTagInTagList);
            }
        }

        public void AppendTag(NBTBase value)
        {
            if (tagType == 0)
            {
                tagType = value.GetId();
            }
            else if (tagType != value.GetId())
            {
                throw new Exception(Sr.AddingInappropriateTagTypesToList);
            }
            tagList.Add(value);
        }

        public NBTBase RemoveTag(int index)
        {
            NBTBase tag = Get(index);
            tagList.RemoveAt(index);
            return tag;
        }

        /// <summary>
        /// Извлекает NBTTagCompound по указанному индексу в списке
        /// </summary>
        public TagCompound GetCompoundTagAt(int index)
        {
            if (index >= 0 && index < tagList.Count)
            {
                NBTBase value = tagList[index];
                if (value.GetId() == 10) return value as TagCompound;
            }
            return new TagCompound();
        }

        public int[] GetIntArray(int index)
        {
            if (index >= 0 && index < tagList.Count)
            {
                NBTBase value = tagList[index];
                if (value.GetId() == 11) return (value as TagIntArray).GetIntArray();
            }
            return new int[0];
        }

        public float GetFloat(int index)
        {
            if (index >= 0 && index < tagList.Count)
            {
                NBTBase value = tagList[index];
                if (value.GetId() == 5) return (value as TagFloat).GetFloat();
            }
            return 0f;
        }

        /// <summary>
        /// Извлекает значение строки тега по указанному индексу в списке
        /// </summary>
        public string GetStringTagAt(int index)
        {
            if (index >= 0 && index < tagList.Count)
            {
                NBTBase value = tagList[index];
                return value.GetId() == 8 ? value.GetString() : value.ToString();
            }
            return "";
        }
    }
}
