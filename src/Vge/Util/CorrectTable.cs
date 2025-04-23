using System.Collections.Generic;
using Vge.NBT;

namespace Vge.Util
{
    /// <summary>
    /// Корректировочная таблица, после загрузки
    /// </summary>
    public class CorrectTable
    {
        /// <summary>
        /// Массив объектов
        /// </summary>
        public string[] LoadObjects { get; private set; } = new string[] { };

        public CorrectTable() { }
        public CorrectTable(string[] objects) => LoadObjects = objects;

        /// <summary>
        /// Корректировка блоков после регистрации и загрузки
        /// </summary>
        public void CorrectRegLoad(IRegTable table)
        {
            List<int> listNull = new List<int>();
            List<string> list = new List<string>();
            List<string> listNew = new List<string>();
            int i, j, count;
            // Проверка тех что загрузили
            count = LoadObjects.Length;
            for (i = 0; i < count; i++)
            {
                list.Add(LoadObjects[i]);
                if (table.Get(LoadObjects[i]) == -1)
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
                    // Новый объект
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

            LoadObjects = list.ToArray();
            // Теперь надо регистрационную таблицу пересортировать
            table.Sort(LoadObjects);
        }

        /// <summary>
        /// Сохранить таблицу
        /// </summary>
        public void Write(string alias, TagCompound nbt)
        {
            if (LoadObjects.Length > 0)
            {
                TagList tagListBlocks = new TagList();
                for (int i = 0; i < LoadObjects.Length; i++)
                {
                    tagListBlocks.AppendTag(new TagString(LoadObjects[i]));
                }
                nbt.SetTag(alias, tagListBlocks);
            }
        }

        /// <summary>
        /// Прочитать таблицу
        /// </summary>
        public void Read(string alias, TagCompound nbt)
        {
            TagList tagListBlocks = nbt.GetTagList(alias, 8);
            int count = tagListBlocks.TagCount();
            LoadObjects = new string[count];
            for (int i = 0; i < count; i++)
            {
                LoadObjects[i] = tagListBlocks.GetStringTagAt(i);
            }
        }
    }
}
