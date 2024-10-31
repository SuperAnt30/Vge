using System.Collections.Generic;
using Vge.NBT;

namespace Vge.World.Block
{
    /// <summary>
    /// Корректировочная таблица блоков, после загрузки
    /// </summary>
    public class CorrectTable
    {
        /// <summary>
        /// Массив блоков
        /// </summary>
        public string[] LoadBlocks { get; private set; } = new string[] { };

        public CorrectTable() { }
        public CorrectTable(string[] blocks) => LoadBlocks = blocks;

        /// <summary>
        /// Корректировка блоков после регистрации и загрузки
        /// </summary>
        public void CorrectRegLoad(BlockRegTable table)
        {
            List<int> listNull = new List<int>();
            List<string> list = new List<string>();
            List<string> listNew = new List<string>();
            int i, j, count;
            // Проверка тех что загрузили
            count = LoadBlocks.Length;
            for (i = 0; i < count; i++)
            {
                list.Add(LoadBlocks[i]);
                if (table.Get(LoadBlocks[i]) == -1)
                {
                    // Отсутствующие помечаем для замены
                    listNull.Add(i);
                }
            }
            // Проверка новых блоков
            count = table.Count;
            string alias;
            bool b;
            for (i = 0; i < count; i++) // цикл новых
            {
                alias = table.GetAlias(i);
                b = false;
                for (j = 0; j < list.Count; j++) // цикл загруженных
                {
                    if (list[j].Equals(alias))
                    {
                        b = true;
                        break;
                    }
                }
                if (!b)
                {
                    // Новый блок
                    if (listNull.Count > 0)
                    {
                        // Вписываем в старый
                        list[listNull[0]] = alias;
                        listNull.RemoveAt(0);
                    }
                    else
                    {
                        // Вписываем в новый
                        list.Add(alias);
                    }
                }
            }

            LoadBlocks = list.ToArray();
            // Теперь надо регистрационную таблицу пересортировать
            table.Sort(LoadBlocks);
        }

        /// <summary>
        /// Сохранить таблицу блоков
        /// </summary>
        public void Write(TagCompound nbt)
        {
            if (LoadBlocks.Length > 0)
            {
                TagList tagListBlocks = new TagList();
                for (int i = 0; i < LoadBlocks.Length; i++)
                {
                    tagListBlocks.AppendTag(new TagString(LoadBlocks[i]));
                }
                nbt.SetTag("TableBlocks", tagListBlocks);
            }
        }

        /// <summary>
        /// Прочитать таблицу блоков
        /// </summary>
        public void Read(TagCompound nbt)
        {
            TagList tagListBlocks = nbt.GetTagList("TableBlocks", 8);
            int count = tagListBlocks.TagCount();
            LoadBlocks = new string[count];
            for (int i = 0; i < count; i++)
            {
                LoadBlocks[i] = tagListBlocks.GetStringTagAt(i);
            }
        }
    }
}
